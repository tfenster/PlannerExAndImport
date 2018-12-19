using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.IdentityModel.Clients.ActiveDirectory;
using PlannerExAndImport.JSON;

namespace PlannerExAndImport
{
    // connects to the MS Graph API (https://graph.microsoft.com) and ex- and imports groups, plans, tasks, details etc.
    public class Planner
    {
        // URLs and settings for the Graph connection
        private const string GRAPH_ENDPOINT = "https://graph.microsoft.com";
        private const string PLANNER_SUB = "/beta/planner/";
        private const string GROUPS_SUB = "/beta/groups/";
        private const string USERS_SUB = "/v1.0/users/";
        private const string RESOURCE_ID = GRAPH_ENDPOINT;
        public static string CLIENT_ID = "";
        public static string TENANT = "";
        private static Dictionary<string, string> users = new Dictionary<string, string>();

        // export a plan and optionally output it as json
        public static Plan Export(bool output = true)
        {
            Plan plan = SelectPlan();

            using (var httpClient = PreparePlannerClient())
            {
                // get all buckets, tasks and task details
                var buckets = GraphResponse<BucketResponse>.Get("plans/" + plan.Id + "/buckets", httpClient).Result.Buckets;
                var tasks = GraphResponse<TaskResponse>.Get("plans/" + plan.Id + "/tasks", httpClient).Result.Tasks;

                foreach (var task in tasks)
                {
                    task.TaskDetail = GraphResponse<TaskDetailResponse>.Get("tasks/" + task.Id + "/details", httpClient).Result;
                }

                // put tasks in buckets so that the plan object has all data hierarchically
                foreach (var bucket in buckets)
                {
                    bucket.Tasks = tasks.Where(t => t.BucketId == bucket.Id).ToArray();
                }

                plan.Buckets = buckets;
            }

            if (output)
                Console.WriteLine(Serialize.ToJson(plan));

            return plan;
        }

        // export a plan and import everything into a new plan
        public static void Import()
        {
            Console.WriteLine("Step 1: Select the plan to export");
            Plan exportedPlan = Export(false);

            Console.WriteLine("Step 2: Select the plan in which you want to import");
            Plan targetPlan = SelectPlan();

            bool addAssignments = Program.GetInput("Do you want to import the assignments (y/n)? This might send email notifications to the assignees. ") == "y";
            using (var httpClient = PreparePlannerClient())
            {
                // buckets and tasks are always added at the beginning, therefore reversing the order when importing, otherwise e.g. the
                // last exported bucket would become the first bucket in the imported plan
                exportedPlan.Buckets = exportedPlan.Buckets.Reverse().ToArray();

                // create buckets and tasks and then set details for the created tasks (can't be done in one step)
                foreach (Bucket bucket in exportedPlan.Buckets)
                {
                    bucket.PlanId = targetPlan.Id;
                    // reset all order hints as the exported values don't work
                    bucket.OrderHint = " !";
                    var newBucket = GraphResponse<Bucket>.Post("buckets", httpClient, bucket).Result;

                    bucket.Tasks = bucket.Tasks.Reverse().ToArray();
                    foreach (PlannerTask task in bucket.Tasks)
                    {
                        task.PlanId = targetPlan.Id;
                        task.BucketId = newBucket.Id;
                        task.OrderHint = " !";

                        // assignments contain the users assigned to a task
                        if (addAssignments)
                        {
                            foreach (Assignment assignment in task.Assignments.Values)
                            {
                                assignment.OrderHint = " !";
                            }
                        }
                        else 
                        {
                            task.Assignments = new Dictionary<string, Assignment>();
                        }
                        var newTask = GraphResponse<PlannerTask>.Post("tasks", httpClient, task).Result;
                        // remember new task id for next loop
                        task.Id = newTask.Id; 
                    }

                    // if we are too quick the created tasks are not available yet
                    Thread.Sleep(2 * 1000);  

                    foreach (PlannerTask task in bucket.Tasks)
                    {
                        var newTaskDetailsResponse = GraphResponse<TaskDetailResponse>.Get("tasks/" + task.Id + "/details", httpClient).Result;
                        foreach (var checklist in task.TaskDetail.Checklist.Values)
                        {
                            checklist.OrderHint = " !";
                        }
                        foreach (var reference in task.TaskDetail.References.Values)
                        {
                            // same as order hint
                            reference.PreviewPriority = " !";
                        }
                        var updatedTaskDetailsResponse = GraphResponse<TaskDetailResponse>.Patch("tasks/" + task.Id + "/details", httpClient, task.TaskDetail, newTaskDetailsResponse.OdataEtag).Result;
                    }
                }
            }

            Console.WriteLine("Import is done");
        }

        public static void ExportToCSV()
        {
            // export the plan
            Console.WriteLine("Select the plan to export");
            Plan exportedPlan = Export(false);

            // convert the plan to CSV
            StringWriter csvString = new StringWriter();
            csvString.WriteLine("Plan;Bucket;Task;Zugewiesen an;Fällig am;Erledigt am;Erledigt von;Erstellt am;Erstellt von");
            foreach (var bucket in exportedPlan.Buckets) {
                foreach (var task in bucket.Tasks) {
                    var completedAt = "";
                    if (task.CompletedDateTime != null) {
                        completedAt = task.CompletedDateTime.ToString();
                    }
                    var completedBy = GetUserForEdBy(task.CompletedBy) ?? "--";

                    var dueAt = "";
                    if (task.DueDateTime != null) {
                        dueAt = task.DueDateTime.ToString();
                    }
                    var createdBy = GetUserForEdBy(task.CreatedBy) ?? "--";

                    var assignedTo = GetAssigned(task.Assignments);

                    if (task.CompletedDateTime == null) {}
                    csvString.WriteLine($"{exportedPlan.Title};{bucket.Name};{task.Title};{assignedTo};{dueAt};{completedAt};{completedBy};{task.CreatedDateTime};{createdBy}");
                }
            }

            File.WriteAllText(exportedPlan.Title + ".csv", csvString.ToString());
        }

        public static void ForgetCredentials()
        {
            AuthenticationContext ctx = new AuthenticationContext("https://login.microsoftonline.com/common");
            ctx.TokenCache.Clear();
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("Vergessen.");
        }

        // allows the user to search for a group, select the right one and then select the right plan
        // idea: if only one group matches or only one plan is in the group, that could be preselected
        private static Plan SelectPlan()
        {
            using (var httpClient = PrepareGroupsClient())
            {
                bool foundGroup = false;
                while (!foundGroup)
                {
                    string groupSearch = Program.GetInput("Please enter the start of the name of the group containing your plan: ");
                    var groups = GraphResponse<GroupResponse>.Get("?$filter=groupTypes/any(c:c+eq+'Unified') and startswith(displayName, '" + groupSearch + "')", httpClient).Result.Groups;
                    if (groups.Length == 0)
                    {
                        Console.WriteLine("Found no matching group");
                    }
                    else
                    {
                        foundGroup = true;

                        Console.WriteLine("Select group:");
                        for (int i = 0; i < groups.Length; i++)
                        {
                            Console.WriteLine("(" + i + ") " + groups[i].DisplayName);
                        }

                        string selectedGroupS = Program.GetInput("Which group do you want to use: ");

                        int selectedGroup = -1;
                        if (int.TryParse(selectedGroupS, out selectedGroup))
                        {
                            var plans = GraphResponse<PlanResponse>.Get(groups[selectedGroup].Id + "/planner/plans", httpClient).Result.Plans;

                            Console.WriteLine("Select plan:");
                            for (int i = 0; i < plans.Length; i++)
                            {
                                Console.WriteLine("(" + i + ") " + plans[i].Title);
                            }

                            string selectedPlanS = Program.GetInput("Which plan do you want to use: ");
                            int selectedPlan = -1;
                            if (int.TryParse(selectedPlanS, out selectedPlan))
                            {
                                return plans[selectedPlan];
                            }
                        }
                    }
                }
            }
            throw new Exception("Please select a plan");
        }

        private static HttpClient PreparePlannerClient()
        {
            return PrepareClient(PLANNER_SUB);
        }

        private static HttpClient PrepareGroupsClient()
        {
            return PrepareClient(GROUPS_SUB);
        }

        private static HttpClient PrepareUsersClient()
        {
            return PrepareClient(USERS_SUB);
        }

        private static HttpClient PrepareClient(string sub)
        {
            var token = GetToken().Result;
            var httpClient = new HttpClient();
            httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token.AccessToken);
            httpClient.BaseAddress = new Uri(GRAPH_ENDPOINT + sub);
            return httpClient;
        }

        // see https://github.com/Azure-Samples/active-directory-dotnet-deviceprofile
        private static async Task<AuthenticationResult> GetToken()
        {
            AuthenticationContext ctx = new AuthenticationContext("https://login.microsoftonline.com/" + TENANT, true);
            if (OperatingSystem.IsWindows())  // EncryptedFileCache only works on Windows
            {
                ctx = new AuthenticationContext("https://login.microsoftonline.com/" + TENANT, true, new EncryptedFileCache());
            }
            AuthenticationResult result = null;
            try
            {
                result = await ctx.AcquireTokenSilentAsync(RESOURCE_ID, CLIENT_ID);
            }
            catch (AdalSilentTokenAcquisitionException)
            {
                result = await GetTokenViaCode(ctx);
            }
            return result;

        }

        static async Task<AuthenticationResult> GetTokenViaCode(AuthenticationContext ctx)
        {
            AuthenticationResult result = null;
            DeviceCodeResult codeResult = await ctx.AcquireDeviceCodeAsync(RESOURCE_ID, CLIENT_ID);
            Console.ResetColor();
            Console.WriteLine("You need to sign in.");
            Console.WriteLine("Message: " + codeResult.Message + "\n");
            result = await ctx.AcquireTokenByDeviceCodeAsync(codeResult);
            return result;
        }

        private static string GetAssigned(Dictionary<string, Assignment> assignments)
        {
            if (assignments.Values.Count > 0)
            {
                var sb = new StringBuilder();
                var delim = "";
                foreach (Assignment assignment in assignments.Values) {
                    sb.Append(delim);
                    sb.Append(GetUserForEdBy(assignment.AssignedBy) ?? "--");
                    delim = ", ";
                }
                return sb.ToString();
            }
            else
            {
                return "--";
            }
        }

        private static string GetUserForEdBy(EdBy edBy)
        {
            if (edBy == null || edBy.User == null)
                return null;
            var id = edBy.User.Id;
            if (users.ContainsKey(id))
                return users[id];
            else
            {
                using (var httpClient = PrepareUsersClient())
                {
                    var user = GraphResponse<UserResponse>.Get(id, httpClient).Result;
                    users.Add(id, user.DisplayName);
                    return user.DisplayName;
                }
            }
        }
    }

    // thanks to https://blog.mariusschulz.com/2017/02/28/detecting-the-operating-system-in-net-core
    public static class OperatingSystem
    {
        public static bool IsWindows() =>
            RuntimeInformation.IsOSPlatform(OSPlatform.Windows);

        public static bool IsMacOS() =>
            RuntimeInformation.IsOSPlatform(OSPlatform.OSX);

        public static bool IsLinux() =>
            RuntimeInformation.IsOSPlatform(OSPlatform.Linux);
    }
}
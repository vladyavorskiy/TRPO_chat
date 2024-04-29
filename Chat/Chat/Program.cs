using Microsoft.AspNetCore.SignalR.Client;

namespace Chat
{
    internal class Program
    {
        //static async Task Main(string[] args)
        //{
        //    var hubConnectionBuilder = new HubConnectionBuilder();

        //    var hubConnection = hubConnectionBuilder.WithUrl("https://localhost:7008/hub")
        //                                            .Build();

        //    await hubConnection.StartAsync();

        //    hubConnection.On("onMessage", (string x) =>
        //    {
        //        Console.WriteLine(x);
        //    });

        //    while (true)
        //    {
        //        var line = Console.ReadLine();

        //        if (string.IsNullOrEmpty(line))
        //            break;

        //        await hubConnection.InvokeAsync("SendMessage", line);
        //    }
        //}








        //static async Task Main(string[] args)
        //{
        //    var hubConnectionBuilder = new HubConnectionBuilder();

        //    var hubConnection = hubConnectionBuilder.WithUrl("https://localhost:7008/hub")
        //        .Build();

        //    await hubConnection.StartAsync();

        //    hubConnection.On("onMessage", (string x) =>
        //    {
        //        Console.WriteLine(x);
        //    });

        //    Console.WriteLine("Enter your name:");
        //    var name = Console.ReadLine();

        //    Console.WriteLine("Enter chat group name:");
        //    var groupName = Console.ReadLine();

        //    await hubConnection.InvokeAsync("JoinGroup", groupName);

        //    while (true)
        //    {
        //        var message = Console.ReadLine();

        //        if (string.IsNullOrEmpty(message))
        //            break;

        //        await hubConnection.InvokeAsync("SendMessage", groupName, $"{name}: {message}");
        //    }
        //}

        //static async Task Main(string[] args)
        //{
        //    var hubConnectionBuilder = new HubConnectionBuilder();

        //    var hubConnection = hubConnectionBuilder.WithUrl("https://localhost:7008/hub")
        //        .Build();

        //    await hubConnection.StartAsync();

        //    hubConnection.On("onMessage", (string x) =>
        //    {
        //        Console.WriteLine(x);
        //    });

        //    Console.WriteLine("Enter your name:");
        //    var name = Console.ReadLine();

        //    Console.WriteLine("Enter chat group name:");
        //    var groupName = Console.ReadLine();

        //    await hubConnection.InvokeAsync("JoinGroup", groupName);

        //    while (true)
        //    {
        //        var message = Console.ReadLine();

        //        if (string.IsNullOrEmpty(message))
        //            break;

        //        await hubConnection.InvokeAsync("SendMessage", groupName, $"{name}: {message}");
        //    }

        //    await hubConnection.InvokeAsync("LeaveGroup", groupName);
        //}












        //static async Task Main(string[] args)
        //{
        //    var hubConnectionBuilder = new HubConnectionBuilder();

        //    var hubConnection = hubConnectionBuilder.WithUrl("https://localhost:7008/hub")
        //                                            .Build();

        //    await hubConnection.StartAsync();

        //    hubConnection.On("onMessage", (string x) =>
        //    {
        //        Console.WriteLine(x);
        //    });

        //    Console.WriteLine("Enter your name:");
        //    var name = Console.ReadLine();

        //    string currentGroup = null;

        //    while (true)
        //    {
        //        // Get user groups
        //        var userGroups = await hubConnection.InvokeAsync<List<string>>("GetUserGroups");

        //        if (currentGroup != null)
        //        {
        //            Console.WriteLine($"You are currently in group '{currentGroup}'. Press Enter to send a message, or '-' to leave the group.");
        //            var input = Console.ReadLine();

        //            if (input == "-")
        //            {
        //                await hubConnection.InvokeAsync("LeaveGroup", currentGroup);
        //                currentGroup = null;
        //                continue;
        //            }

        //            // Send message to the current group
        //            await hubConnection.InvokeAsync("SendMessage", currentGroup, $"{name}: {input}");
        //            continue;
        //        }

        //        // Display user groups
        //        Console.WriteLine("Your groups:");
        //        for (int i = 0; i < userGroups.Count; i++)
        //        {
        //            Console.WriteLine($"{i + 1}. {userGroups[i]}");
        //        }

        //        // Display option to create a group
        //        Console.WriteLine("0. Create a new group");

        //        // Display option to join a group
        //        Console.WriteLine("Enter the name or number of the group to join:");

        //        var inputGroup = Console.ReadLine();

        //        if (string.IsNullOrEmpty(inputGroup))
        //            continue;

        //        if (inputGroup == "-")
        //        {
        //            Console.WriteLine("You are not in any group.");
        //            continue;
        //        }

        //        if (inputGroup == "0")
        //        {
        //            // Create a new group
        //            Console.WriteLine("Enter the name of the new group:");
        //            var newGroupName = Console.ReadLine();
        //            await hubConnection.InvokeAsync("CreateGroup", newGroupName);
        //            await hubConnection.InvokeAsync("JoinGroup", newGroupName);
        //        }
        //        else if (int.TryParse(inputGroup, out int groupIndex) && groupIndex > 0 && groupIndex <= userGroups.Count)
        //        {
        //            // Join selected group
        //            currentGroup = userGroups[groupIndex - 1];
        //            Console.WriteLine($"You are now in group '{currentGroup}'. Press Enter to send a message, or '-' to leave the group.");
        //        }
        //        else
        //        {
        //            // Attempt to join the group
        //            await hubConnection.InvokeAsync("AddToGroup", inputGroup);
        //            Console.WriteLine($"Trying to join group '{inputGroup}'.");
        //        }
        //    }
        //}


        //static async Task Main(string[] args)
        //{
        //    var hubConnectionBuilder = new HubConnectionBuilder();

        //    var hubConnection = hubConnectionBuilder.WithUrl("https://localhost:7008/hub")
        //                                            .Build();

        //    await hubConnection.StartAsync();

        //    hubConnection.On("onMessage", (string x) =>
        //    {
        //        Console.WriteLine(x);
        //    });

        //    Console.WriteLine("Enter your name:");
        //    var name = Console.ReadLine();

        //    string currentGroup = null;

        //    while (true)
        //    {
        //        // Get user groups
        //        var userGroups = await hubConnection.InvokeAsync<List<string>>("GetUserGroups");

        //        if (currentGroup != null)
        //        {
        //            //Console.WriteLine($"You are currently in group '{currentGroup}'. Press Enter to send a message, or '-' to leave the group.");
        //            var input = Console.ReadLine();

        //            if (input == "-")
        //            {
        //                await hubConnection.InvokeAsync("LeaveGroup", currentGroup);
        //                currentGroup = null;
        //                continue;
        //            }

        //            // Send message to the current group
        //            await hubConnection.InvokeAsync("SendMessage", currentGroup, $"{name}: {input}");
        //            continue;
        //        }

        //        // Display user groups
        //        Console.WriteLine("Your groups:");
        //        for (int i = 0; i < userGroups.Count; i++)
        //        {
        //            Console.WriteLine($"{i + 1}. {userGroups[i]}");
        //        }

        //        // Display option to create a group
        //        Console.WriteLine("0. Create a new group");

        //        // Display option to join a group
        //        Console.WriteLine("Enter the name or number of the group to join:");

        //        var inputGroup = Console.ReadLine();

        //        if (string.IsNullOrEmpty(inputGroup))
        //            continue;

        //        if (inputGroup == "-")
        //        {
        //            Console.WriteLine("You are not in any group.");
        //            continue;
        //        }

        //        if (inputGroup == "0")
        //        {
        //            // Create a new group
        //            Console.WriteLine("Enter the name of the new group:");
        //            var newGroupName = Console.ReadLine();
        //            await hubConnection.InvokeAsync("CreateGroup", newGroupName);
        //            await hubConnection.InvokeAsync("JoinGroup", newGroupName);
        //        }
        //        else if (int.TryParse(inputGroup, out int groupIndex) && groupIndex > 0 && groupIndex <= userGroups.Count)
        //        {
        //            // Join selected group
        //            currentGroup = userGroups[groupIndex - 1];
        //            Console.WriteLine($"You are now in group '{currentGroup}'. Press Enter to send a message, or '-' to leave the group.");
        //        }
        //        else
        //        {
        //            // Attempt to join the group
        //            await hubConnection.InvokeAsync("AddToGroup", inputGroup);
        //            Console.WriteLine($"Trying to join group '{inputGroup}'.");
        //        }
        //    }
        //}





        //static async Task Main(string[] args)
        //{
        //    var hubConnectionBuilder = new HubConnectionBuilder();

        //    var hubConnection = hubConnectionBuilder.WithUrl("https://localhost:7008/hub")
        //                                            .Build();

        //    await hubConnection.StartAsync();

        //    hubConnection.On("onMessage", (string x) =>
        //    {
        //        Console.WriteLine(x);
        //    });

        //    Console.WriteLine("Enter your name:");
        //    var name = Console.ReadLine();

        //    string currentGroup = null;

        //    while (true)
        //    {
        //        if (currentGroup == null)
        //        {
        //            Console.Clear();
        //            // Get user groups
        //            var userGroups = await hubConnection.InvokeAsync<List<string>>("GetUserGroups");

        //            // Display user groups only when not in a group
        //            if (userGroups.Count > 0)
        //            {
        //                Console.WriteLine("Your groups:");
        //                for (int i = 0; i < userGroups.Count; i++)
        //                {
        //                    Console.WriteLine($"{i + 1}. {userGroups[i]}");
        //                }
        //            }
        //            else
        //            {
        //                Console.WriteLine("You are not in any group.");
        //            }

        //            // Display option to create a group
        //            Console.WriteLine("0. Create a new group");

        //            // Display option to join a group
        //            Console.WriteLine("Enter the name or number of the group to join:");

        //            var inputGroup = Console.ReadLine();

        //            if (string.IsNullOrEmpty(inputGroup))
        //                continue;

        //            if (inputGroup == "-")
        //            {
        //                Console.WriteLine("You are not in any group.");
        //                continue;
        //            }

        //            if (inputGroup == "0")
        //            {
        //                // Create a new group
        //                Console.Clear();
        //                Console.WriteLine("Enter the name of the new group:");
        //                var newGroupName = Console.ReadLine();
        //                await hubConnection.InvokeAsync("CreateGroup", newGroupName);
        //                //await hubConnection.InvokeAsync("AddToGroup", newGroupName);
        //            }
        //            else if (int.TryParse(inputGroup, out int groupIndex) && groupIndex > 0 && groupIndex <= userGroups.Count)
        //            {
        //                // Join selected group
        //                Console.Clear();
        //                currentGroup = userGroups[groupIndex - 1];
        //                Console.WriteLine($"You are now in group '{currentGroup}'. Press Enter to send a message, or '-' to leave the group.");
        //                await hubConnection.InvokeAsync("JoinGroup", currentGroup);
        //            }
        //            else
        //            {
        //                // Attempt to join the group
        //                Console.Clear();
        //                Console.WriteLine($"Trying to join group '{inputGroup}'.");
        //                await hubConnection.InvokeAsync("JoinGroup", inputGroup);
        //            }
        //        }
        //        else
        //        {
        //            //Console.WriteLine($"You are currently in group '{currentGroup}'. Press Enter to send a message, or '-' to leave the group.");
        //            var input = Console.ReadLine();

        //            if (input == "-")
        //            {
        //                Console.Clear();
        //                await hubConnection.InvokeAsync("LeaveGroup", currentGroup);
        //                currentGroup = null;
        //                continue;
        //            }

        //            // Send message to the current group
        //            await hubConnection.InvokeAsync("SendMessage", currentGroup, $"{name}: {input}");
        //        }
        //    }
        //}











        static async Task Main(string[] args)
        {
            var hubConnectionBuilder = new HubConnectionBuilder();

            var hubConnection = hubConnectionBuilder.WithUrl("https://localhost:7008/hub")
                                                    .Build();

            await hubConnection.StartAsync();

            hubConnection.On("onMessage", (string x) =>
            {
                Console.WriteLine(x);
            });

            Console.WriteLine("Enter your name:");
            var name = Console.ReadLine();

            string currentGroup = null;

            while (true)
            {
                if (currentGroup == null)
                {
                    Console.Clear();
                    // Get user groups
                    var userGroups = await hubConnection.InvokeAsync<List<string>>("GetUserGroups");

                    // Display user groups only when not in a group
                    if (userGroups.Count > 0)
                    {
                        Console.WriteLine("Your groups:");
                        for (int i = 0; i < userGroups.Count; i++)
                        {
                            Console.WriteLine($"{i + 1}. {userGroups[i]}");
                        }
                    }
                    else
                    {
                        Console.WriteLine("You are not in any group.");
                    }

                    // Display option to create a group
                    Console.WriteLine("0. Create a new group");

                    // Display option to join a group
                    Console.WriteLine("Enter the name or number of the group to join:");

                    var inputGroup = Console.ReadLine();

                    if (string.IsNullOrEmpty(inputGroup))
                        continue;

                    if (inputGroup == "-")
                    {
                        Console.WriteLine("You are not in any group.");
                        continue;
                    }

                    if (inputGroup == "0")
                    {
                        // Create a new group
                        Console.Clear();
                        Console.WriteLine("Enter the name of the new group:");
                        var newGroupName = Console.ReadLine();
                        await hubConnection.InvokeAsync("CreateGroup", newGroupName);
                        await hubConnection.InvokeAsync("JoinGroup", newGroupName);
                        userGroups = await hubConnection.InvokeAsync<List<string>>("GetUserGroups");
                        currentGroup = userGroups[userGroups.Count - 1];
                        Console.WriteLine($"You are now in group '{newGroupName}'. Press Enter to send a message, or '-' to leave the group.");
                    }
                    else if (int.TryParse(inputGroup, out int groupIndex) && groupIndex > 0 && groupIndex <= userGroups.Count)
                    {
                        // Join selected group
                        Console.Clear();
                        currentGroup = userGroups[groupIndex - 1];
                        Console.WriteLine($"You are now in group '{currentGroup}'. Press Enter to send a message, or '-' to leave the group.");
                        await hubConnection.InvokeAsync("JoinGroup", currentGroup);
                    }
                    else
                    {
                        // Attempt to join the group
                        Console.Clear();
                        Console.WriteLine($"Trying to join group '{inputGroup}'.");
                        int joinResult = await hubConnection.InvokeAsync<int>("JoinGroup", inputGroup);
                        if (joinResult == 0)
                        {
                            Console.WriteLine($"Group {inputGroup} does not exist");
                            continue;
                        }
                        else
                        {
                            userGroups = await hubConnection.InvokeAsync<List<string>>("GetUserGroups");
                            currentGroup = userGroups[userGroups.Count - 1];
                            //Console.WriteLine($"You are now in group '{inputGroup}'. Press Enter to send a message, or '-' to leave the group.");
                        }
                    }
                }
                else
                {
                    //Console.WriteLine($"You are currently in group '{currentGroup}'. Press Enter to send a message, or '-' to leave the group.");
                    var input = Console.ReadLine();

                    if (input == "-")
                    {
                        Console.Clear();
                        await hubConnection.InvokeAsync("LeaveGroup", currentGroup);
                        currentGroup = null;
                        continue;
                    }

                    // Send message to the current group
                    await hubConnection.InvokeAsync("SendMessage", currentGroup, $"{name}: {input}");
                }
            }
        }
    }
}

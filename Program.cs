using System;
using System.Collections.Generic;
using System.Linq;
class Solution
{
    /*
     * Complete the function below.
     */
    const string LIST = "LIST";
    const string DEPEND = "DEPEND";
    const string INSTALL = "INSTALL";
    const string REMOVE = "REMOVE";
    const string END = "END";

    static void doIt(string[] input)
    {
        var graph = new Dictionary<string, HashSet<string>>();
        var inverseGraph = new Dictionary<string, HashSet<string>>();
        var installedComponents = new HashSet<string>();

        foreach (string line in input)
        {
            string[] subStrings = line.Split(' ');
            if (subStrings.Length < 1)
            {
                Console.WriteLine("Incorrect input, ignoring");
                continue;
            }

            string command = subStrings[0].Trim();
            switch (command)
            {
                case DEPEND:
                    HandleDependCommand(subStrings, graph, inverseGraph);
                    break;
                case INSTALL:
                    HandleInstallCommand(subStrings, graph, installedComponents);
                    break;
                case LIST:
                    HandleListCommand(installedComponents);
                    break;
                case REMOVE:
                    HandleRemoveCommand(subStrings, graph, inverseGraph, installedComponents);
                    break;
                case END:
                    Console.WriteLine(END);
                    break;
            }
        }
    }

    /*
    
    GRAPH =>
    TELNET -> TCPIP, NETCARD
    TCPIP -> NETCARD
    DNS-> TCPIP, NETCARD
    BROWSER -> TCPIP, HTML
    
    INVERSEGRAPH => 
    TCPIP -> TELNET, DNS, BROWSER
    NETCARD -> TELNET, TCIP, DNS
    HTML -> BROWSER
    */
    static void HandleDependCommand(
        string[] subStrings,
        Dictionary<string, HashSet<string>> graph,
        Dictionary<string, HashSet<string>> inverseGraph
        )
    {
        var component = subStrings[1].Trim();
        var requirements = new HashSet<string>();
        for (int i = 2; i < subStrings.Length; i++)
        {
            var currentComponent = subStrings[i].Trim();
            requirements.Add(currentComponent);
        }

        var formattedRequirements = string.Join(" ", requirements);
        Console.WriteLine($"{DEPEND} {component} {formattedRequirements}");

        bool isCausingCyle = doesCycleExist(component, requirements, graph, inverseGraph);
        if (!isCausingCyle)
        {
            graph[component] = requirements;
            foreach (var currentComponent in requirements)
            {
                if (inverseGraph.ContainsKey(currentComponent))
                {
                    inverseGraph[currentComponent].Add(component);
                }
                else
                {
                    inverseGraph[currentComponent] = new HashSet<string> { component };
                }
            }
        }
    }

    static void HandleInstallCommand(
        string[] subStrings,
        Dictionary<string, HashSet<string>> graph,
        HashSet<string> installedSet)
    {
        var component = subStrings[1].Trim();
        Console.WriteLine($"{INSTALL} {component}");
        var stack = new Stack<string>();
        var queue = new Queue<string>();
        queue.Enqueue(component);
        while (queue.Any())
        {
            var currentComponent = queue.Dequeue();
            if (graph.ContainsKey(currentComponent))
            {
                var requirements = graph[currentComponent];
                foreach (var requirement in requirements)
                {
                    if (!installedSet.Contains(requirement))
                    {
                        queue.Enqueue(requirement);
                    }
                }
            }
            stack.Push(currentComponent);
        }
        while (stack.Any())
        {
            var current = stack.Pop();
            if(!installedSet.Contains(current))
            {
                Console.WriteLine($"Installing {current}");
                installedSet.Add(current);
            }            
        }
    }

    static void HandleListCommand(HashSet<string> installedSet)
    {
        Console.WriteLine($"{LIST}");
        foreach (string component in installedSet)
        {
            Console.WriteLine($"{component}");
        }
    }

    static void HandleRemoveCommand(
        string[] subStrings,
        Dictionary<string, HashSet<string>> graph,
        Dictionary<string, HashSet<string>> inverseGraph,
        HashSet<string> installedSet)
    {
        var component = subStrings[1].Trim();
        Console.WriteLine($"{REMOVE} {component}");
        if (!installedSet.Contains(component))
        {
            Console.WriteLine($"{component} is not installed, ignoring command");
            return;
        }

        bool canRemove = false;
        var stack = new Stack<string>();
        stack.Push(component);

        var itemsRemovable = new HashSet<string>();
        while (stack.Any())
        {
            var current = stack.Pop();
            canRemove = canBeRemoved(current, inverseGraph, installedSet, itemsRemovable);
            if (canRemove)
            {
                itemsRemovable.Add(current);
            }
            else
            {
                continue;
                //No need to check its dependencies
            }            

            if (graph.ContainsKey(current))
            {
                var candidatesForRemoval = graph[current];
                foreach (var each in candidatesForRemoval)
                {
                    if (installedSet.Contains(each))
                    {
                        stack.Push(each);
                    }
                }
            }
        }

        if(itemsRemovable.Contains(component))
        {
            foreach (var item in itemsRemovable)
            {
                Console.WriteLine($"Removing {item}");
                installedSet.Remove(item);
            }
        }
        else
        {
            Console.WriteLine($"{component} is still needed");
        }
    }

    static bool canBeRemoved(
        string current,
        Dictionary<string, HashSet<string>> inverseGraph,
        HashSet<string> installedSet,
        HashSet<string> itemsRemovable)
    {
        if (inverseGraph.ContainsKey(current))
        {
            //Some components rely on current, check if these are installed
            var dependent = inverseGraph[current];
            foreach (var req in dependent)
            {
                if (installedSet.Contains(req) && !itemsRemovable.Contains(req)) 
                // The item is installed and not eligible for removal
                {
                    return false;
                }
            }
        }

        return true;

    }

    static bool doesCycleExist(
        string component,
        HashSet<string> requirements,
        Dictionary<string, HashSet<string>> graph,
        Dictionary<string, HashSet<string>> inverseGraph)
    {
        var stack = new Stack<string>();
        foreach (var requirement in requirements)
        {
            stack.Push(requirement);
        }
        var allRequirements = new HashSet<string>();
        while (stack.Any())
        {
            var current = stack.Pop();
            allRequirements.Add(current);
            if (graph.ContainsKey(current))
            {
                foreach (var each in graph[current])
                {
                    if (!allRequirements.Contains(each))
                    {
                        stack.Push(each);
                    }
                }
            }
        }

        foreach (var req in allRequirements)
        {
            if (graph.ContainsKey(req))
            {
                var exists = graph[req].Contains(component);
                if (exists) // cycle found
                {
                    Console.WriteLine($"{req} depends on {component}, ignoring command");
                    return true;
                }
            }
        }

        return false;

    }

    static void Main(String[] args)
    {
        var list = new List<string> {
            "DEPEND TELNET TCPIP NETCARD",
            "DEPEND TCPIP NETCARD",
            "DEPEND NETCARD TCPIP",
            "DEPEND DNS TCPIP NETCARD",
            "DEPEND BROWSER TCPIP HTML",
            "INSTALL NETCARD",
            "INSTALL TELNET",
            "INSTALL foo",
            "REMOVE NETCARD",
            "INSTALL BROWSER",
            "INSTALL DNS",
            "LIST",
            "REMOVE TELNET",
            "REMOVE NETCARD",
            "REMOVE DNS",
            "REMOVE NETCARD",
            "INSTALL NETCARD",
            "REMOVE TCPIP",
            "REMOVE BROWSER",
            "REMOVE TCPIP",
            "LIST",
            "END"
        };
        doIt(list.ToArray());

    }
}

/**
Approach -
1) USING DEPEND - Build a dependency graph where key is the installed component, value is list of required components. Prevent Cycle in graph. Ignore if the code causes a cycle.
2) INSTALL - Use the graph to build sequence of installs
3) REMOVE - Remove component iff nothing depends on it. If possible to remove, try to remove other components that were needed due to this component. If not possible print "X is still needed"
4) LIST - print the graph (key - values)

*/
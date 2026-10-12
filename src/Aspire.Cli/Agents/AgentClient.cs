// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Aspire.Cli.Agents.ClaudeCode;
using Aspire.Cli.Resources;

namespace Aspire.Cli.Agents;

/// <summary>
/// A supported client group and its shared or agent-specific skill destinations.
/// </summary>
internal sealed class AgentClient
{
    private static readonly string s_universalSkillsDirectory = Path.Combine(".agents", "skills");

    public static readonly AgentClient Copilot = new()
    {
        Id = "copilot",
        DisplayName = AgentCommandStrings.Environment_Copilot,
        Environments = AgentEnvironment.CopilotCli | AgentEnvironment.CopilotApp | AgentEnvironment.VsCode
    };

    public static readonly AgentClient ClaudeCode = new()
    {
        Id = "claude",
        DisplayName = AgentCommandStrings.Environment_ClaudeCode,
        Environments = AgentEnvironment.ClaudeCode,
        ProjectSkillsDirectory = Path.Combine(".claude", "skills"),
        GlobalSkillsDirectory = static (context, environment) =>
            Path.Combine(ClaudeCodePaths.GetConfigDirectory(context, environment), "skills")
    };

    public static readonly AgentClient OpenCode = new()
    {
        Id = "opencode",
        DisplayName = AgentCommandStrings.Environment_OpenCode,
        Environments = AgentEnvironment.OpenCode
    };

    public static IReadOnlyList<AgentClient> All { get; } = [Copilot, ClaudeCode, OpenCode];

    private AgentClient()
    {
    }

    public required string Id { get; init; }
    public required string DisplayName { get; init; }
    public required AgentEnvironment Environments { get; init; }
    public string ProjectSkillsDirectory { get; init; } = s_universalSkillsDirectory;
    public Func<CliExecutionContext, IEnvironment, string> GlobalSkillsDirectory { get; init; } =
        static (context, _) => Path.Combine(context.HomeDirectory.FullName, s_universalSkillsDirectory);

    public bool Includes(AgentEnvironment environments) => (Environments & environments) != AgentEnvironment.None;

    public string GetSkillBaseDirectory(AgentConfigurationScope scope, DirectoryInfo? workspaceRoot, CliExecutionContext executionContext, IEnvironment environment)
    {
        return scope is AgentConfigurationScope.Project
            ? Path.Combine(AgentPath.RequireWorkspaceRoot(workspaceRoot).FullName, ProjectSkillsDirectory)
            : GlobalSkillsDirectory(executionContext, environment);
    }

    public override string ToString() => Id;
}

internal enum AgentConfigurationScope
{
    Project,
    Global
}

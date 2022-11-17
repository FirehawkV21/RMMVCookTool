var target = Argument("target", "Test");
var configuration = Argument("configuration", "Release");

//////////////////////////////////////////////////////////////////////
// TASKS
//////////////////////////////////////////////////////////////////////

Task("Clean")
    .WithCriteria(c => HasArgument("rebuild"))
    .Does(() =>
{
    CleanDirectory($"./RMMVCookTool.CLI/bin/{configuration}");
    CleanDirectory($"./RMMVCookTool.CLI/obj/{configuration}");
    CleanDirectory($"./RMMVCookTool.Core/bin/{configuration}");
    CleanDirectory($"./RMMVCookTool.Core/obj/{configuration}");
    CleanDirectory($"./RMMVCookTool.GUI/bin/{configuration}");
    CleanDirectory($"./RMMVCookTool.GUI/obj/{configuration}");
});

Task("BuildUI")
    .IsDependentOn("Clean")
    .WithCriteria(c => HasArgument("buildUi"))
    .Does(() =>
{
    DotNetBuild("./RMMVCookTool.GUI", new DotNetBuildSettings
    {
        Configuration = configuration,
        Architecture = "x64"
    });
});

Task("BuildCLI")
    .IsDependentOn("Clean")
    .WithCriteria(c => HasArgument("buildCli"))
    .Does(() =>
{
    DotNetBuild("./RMMVCookTool.CLI", new DotNetBuildSettings
    {
        Configuration = configuration,
    });
});

Task("PublishUI")
    .IsDependentOn("Clean")
    .WithCriteria(c => HasArgument("publishUi"))
    .Does(() =>
{
    DotNetPublish("./RMMVCookTool.GUI", new DotNetPublishSettings
    {
        Configuration = configuration,
        SelfContained = true,
        ArgumentCustomization = args => args.Append("-a x64")
});
});

Task("PublishCLI")
    .IsDependentOn("Clean")
    .WithCriteria(c => HasArgument("buildCli"))
    .Does(() =>
{
    DotNetBuild("./RMMVCookTool.CLI", new DotNetBuildSettings
    {
        Configuration = configuration,
        SelfContained = true,
        ArgumentCustomization = args => args.Append("-a x64")
    });
});

Task("PublishUIOnArm")
    .IsDependentOn("Clean")
    .WithCriteria(c => HasArgument("publishUi"))
    .Does(() =>
{
    DotNetPublish("./RMMVCookTool.GUI", new DotNetPublishSettings
    {
        Configuration = configuration,
        SelfContained = true,
        ArgumentCustomization = args => args.Append("-a arm64")
    });
});

Task("PublishCLIOnArm")
    .IsDependentOn("Clean")
    .WithCriteria(c => HasArgument("buildCli"))
    .Does(() =>
{
    DotNetBuild("./RMMVCookTool.CLI", new DotNetBuildSettings
    {
        Configuration = configuration,
        SelfContained = true,
        ArgumentCustomization = args => args.Append("-a arm64")
    });
});

//////////////////////////////////////////////////////////////////////
// EXECUTION
//////////////////////////////////////////////////////////////////////

RunTarget(target);
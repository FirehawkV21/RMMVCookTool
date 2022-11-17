var target = Argument("target", "Cleanup");
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
    .WithCriteria(c => HasArgument("publishCli"))
    .Does(() =>
{
    DotNetPublish("./RMMVCookTool.CLI", new DotNetPublishSettings
    {
        Configuration = configuration,
        SelfContained = true,
        ArgumentCustomization = args => args.Append("-a x64")
    });
});

Task("PublishUIOnArm")
    .IsDependentOn("Clean")
    .WithCriteria(c => HasArgument("publishUiOnArm"))
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
    .WithCriteria(c => HasArgument("publishCliOnArm"))
    .Does(() =>
{
    DotNetPublish("./RMMVCookTool.CLI", new DotNetPublishSettings
    {
        Configuration = configuration,
        SelfContained = true,
        ArgumentCustomization = args => args.Append("-a arm64")
    });
});

Task("Cleanup")
    .IsDependentOn("Clean")
    .IsDependentOn("BuildUI")
    .IsDependentOn("BuildCLI")
    .IsDependentOn("PublishUI")
    .IsDependentOn("PublishCLI")
    .IsDependentOn("PublishUIOnArm")
    .IsDependentOn("PublishCLIOnArm");

//////////////////////////////////////////////////////////////////////
// EXECUTION
//////////////////////////////////////////////////////////////////////

RunTarget(target);
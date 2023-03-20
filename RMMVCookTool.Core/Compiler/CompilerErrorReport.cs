namespace RMMVCookTool.Core.Compiler;
public record CompilerErrorReport
{
    public int ErrorCode { get; set; }
    public string ErrorMessage { get; set; }
}

class Handler
{
    private uint pipid;
    public Handler(uint pipid) {
        this.pipid = pipid;
    }
    public bool HandlerRoutine(uint dwCtrlType)
    {
        if (dwCtrlType == 0)
        {
            Helper.GenerateConsoleCtrlEvent(1, pipid);
            return true;
        }
        return false;
    }
}
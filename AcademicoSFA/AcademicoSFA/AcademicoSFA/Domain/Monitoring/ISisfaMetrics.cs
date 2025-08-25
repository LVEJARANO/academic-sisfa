namespace AcademicoSFA.Domain.Monitoring
{
    public interface ISisfaMetrics
    {
        void RecordLoginAttempt(bool success, string email);
        void RecordConsultaNotasTime(double seconds);
        void RecordActiveUsers(int count);
        void RecordDatabaseOperation(string operation, bool success, double duration);
        void RecordPageView(string pageName);
    }
}

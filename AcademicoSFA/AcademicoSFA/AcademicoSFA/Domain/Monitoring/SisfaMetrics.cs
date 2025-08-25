using Prometheus;

namespace AcademicoSFA.Domain.Monitoring
{
    public class SisfaMetrics : ISisfaMetrics
    {
        private readonly Counter _loginAttempts;
        private readonly Histogram _consultaNotasTime;
        private readonly Gauge _activeUsers;
        private readonly Counter _databaseOperations;
        private readonly Histogram _databaseOperationDuration;
        private readonly Counter _pageViews;
        public SisfaMetrics()
        {
            // Métrica para intentos de login
            _loginAttempts = Metrics
                .CreateCounter("sisfa_login_attempts_total", "Total login attempts", new[] { "status", "email" });

            // Métrica para tiempo de consulta de notas
            _consultaNotasTime = Metrics
                .CreateHistogram("sisfa_consulta_notas_duration_seconds", "Time spent on grade consultations");

            // Métrica para usuarios activos
            _activeUsers = Metrics
                .CreateGauge("sisfa_active_users", "Number of active users");

            // Métricas para operaciones de base de datos
            _databaseOperations = Metrics
                .CreateCounter("sisfa_database_operations_total", "Total database operations", new[] { "operation", "status" });

            _databaseOperationDuration = Metrics
                .CreateHistogram("sisfa_database_operation_duration_seconds", "Database operation duration", new[] { "operation" });

            // Métrica para vistas de página
            _pageViews = Metrics
                .CreateCounter("sisfa_page_views_total", "Total page views", new[] { "page" });
        }

        public void RecordLoginAttempt(bool success, string email)
        {
            _loginAttempts.WithLabels(success ? "success" : "failure", email).Inc();
        }

        public void RecordConsultaNotasTime(double seconds)
        {
            _consultaNotasTime.Observe(seconds);
        }

        public void RecordActiveUsers(int count)
        {
            _activeUsers.Set(count);
        }

        public void RecordDatabaseOperation(string operation, bool success, double duration)
        {
            _databaseOperations.WithLabels(operation, success ? "success" : "failure").Inc();
            _databaseOperationDuration.WithLabels(operation).Observe(duration);
        }

        public void RecordPageView(string pageName)
        {
            _pageViews.WithLabels(pageName).Inc();
        }
    }
}

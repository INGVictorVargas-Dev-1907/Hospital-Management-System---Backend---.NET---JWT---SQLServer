using HospitalManagementSystem.DTOs;

namespace HospitalManagementSystem.Services
{
    /// <summary>
    /// Define los contratos del servicio de gestión de citas médicas.
    /// Esta interfaz declara las operaciones principales relacionadas con la creación,
    /// consulta, actualización y cancelación de citas en el sistema hospitalario.
    /// </summary>
    public interface IAppointmentService
    {
        /// <summary>
        /// Obtiene todas las citas registradas en el sistema.
        /// </summary>
        /// <returns>
        /// Una colección enumerable de objetos <see cref="AppointmentDto"/> que representan todas las citas existentes.
        /// </returns>
        Task<IEnumerable<AppointmentDto>> GetAllAppointmentsAsync();

        /// <summary>
        /// Obtiene todas las citas asociadas a un paciente específico.
        /// </summary>
        /// <param name="patientId">Identificador único del paciente.</param>
        /// <returns>
        /// Una colección enumerable de <see cref="AppointmentDto"/> correspondientes al paciente especificado.
        /// </returns>
        Task<IEnumerable<AppointmentDto>> GetAppointmentsByPatientAsync(Guid patientId);

        /// <summary>
        /// Obtiene todas las citas asignadas a un médico específico.
        /// </summary>
        /// <param name="doctorId">Identificador único del médico.</param>
        /// <returns>
        /// Una colección enumerable de <see cref="AppointmentDto"/> correspondientes al médico especificado.
        /// </returns>
        Task<IEnumerable<AppointmentDto>> GetAppointmentsByDoctorAsync(Guid doctorId);

        /// <summary>
        /// Obtiene una cita específica a partir de su identificador único.
        /// </summary>
        /// <param name="id">Identificador único de la cita.</param>
        /// <returns>
        /// Un objeto <see cref="AppointmentDto"/> si la cita existe; de lo contrario, <c>null</c>.
        /// </returns>
        Task<AppointmentDto?> GetAppointmentByIdAsync(Guid id);

        /// <summary>
        /// Crea una nueva cita en el sistema.
        /// </summary>
        /// <param name="createAppointmentDto">
        /// Objeto que contiene la información necesaria para crear la cita,
        /// como el paciente, el médico, la fecha y la descripción.
        /// </param>
        /// <returns>
        /// El objeto <see cref="AppointmentDto"/> que representa la cita creada.
        /// </returns>
        Task<AppointmentDto> CreateAppointmentAsync(CreateAppointmentDto createAppointmentDto);

        /// <summary>
        /// Actualiza el estado de una cita existente (por ejemplo: "Confirmada", "Completada", "Cancelada").
        /// </summary>
        /// <param name="id">Identificador único de la cita.</param>
        /// <param name="status">Nuevo estado a asignar a la cita.</param>
        /// <returns>
        /// <c>true</c> si la actualización fue exitosa; de lo contrario, <c>false</c>.
        /// </returns>
        Task<bool> UpdateAppointmentStatusAsync(Guid id, string status);

        /// <summary>
        /// Cancela una cita existente.
        /// </summary>
        /// <param name="id">Identificador único de la cita a cancelar.</param>
        /// <returns>
        /// <c>true</c> si la cancelación fue exitosa; de lo contrario, <c>false</c>.
        /// </returns>
        Task<bool> CancelAppointmentAsync(Guid id);

        /// <summary>
        /// Obtiene todas las citas que tienen un estado específico.
        /// </summary>
        /// <param name="status">Estado de las citas a filtrar (por ejemplo: "Pendiente", "Completada").</param>
        /// <returns>
        /// Una colección enumerable de <see cref="AppointmentDto"/> que coinciden con el estado solicitado.
        /// </returns>
        Task<IEnumerable<AppointmentDto>> GetAppointmentsByStatusAsync(string status);
    }
}

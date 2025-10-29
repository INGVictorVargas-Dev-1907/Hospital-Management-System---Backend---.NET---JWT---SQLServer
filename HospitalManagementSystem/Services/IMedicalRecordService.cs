using HospitalManagementSystem.DTOs;

namespace HospitalManagementSystem.Services
{
    /// <summary>
    /// Define los contratos del servicio encargado de gestionar los historiales y registros médicos.
    /// Esta interfaz proporciona los métodos necesarios para consultar, crear y exportar la información
    /// médica de los pacientes dentro del sistema hospitalario.
    /// </summary>
    public interface IMedicalRecordService
    {
        /// <summary>
        /// Obtiene el historial médico completo de un paciente específico.
        /// </summary>
        /// <param name="patientId">Identificador único del paciente.</param>
        /// <returns>
        /// Una colección enumerable de <see cref="MedicalRecordDto"/> que contiene los registros médicos del paciente.
        /// </returns>
        Task<IEnumerable<MedicalRecordDto>> GetPatientMedicalHistoryAsync(Guid patientId);

        /// <summary>
        /// Obtiene un registro médico individual a partir de su identificador único.
        /// </summary>
        /// <param name="id">Identificador único del registro médico.</param>
        /// <returns>
        /// Un objeto <see cref="MedicalRecordDto"/> si se encuentra el registro; de lo contrario, <c>null</c>.
        /// </returns>
        Task<MedicalRecordDto?> GetMedicalRecordByIdAsync(Guid id);

        /// <summary>
        /// Crea un nuevo registro médico asociado a un paciente.
        /// </summary>
        /// <param name="createMedicalRecordDto">
        /// Objeto que contiene la información necesaria para crear el registro médico, como diagnósticos,
        /// tratamientos, observaciones y fecha de atención.
        /// </param>
        /// <returns>
        /// El objeto <see cref="MedicalRecordDto"/> que representa el registro médico recién creado.
        /// </returns>
        Task<MedicalRecordDto> CreateMedicalRecordAsync(CreateMedicalRecordDto createMedicalRecordDto);

        /// <summary>
        /// Genera un archivo PDF con el historial médico de un paciente, permitiendo filtrar por un rango de fechas.
        /// </summary>
        /// <param name="patientId">Identificador único del paciente.</param>
        /// <param name="startDate">Fecha inicial del rango a filtrar (opcional).</param>
        /// <param name="endDate">Fecha final del rango a filtrar (opcional).</param>
        /// <returns>
        /// Un arreglo de bytes que representa el archivo PDF generado con el historial médico del paciente.
        /// </returns>
        Task<byte[]> GenerateMedicalHistoryPdfAsync(Guid patientId, DateTime? startDate = null, DateTime? endDate = null);

        /// <summary>
        /// Obtiene todos los registros médicos almacenados en el sistema.
        /// </summary>
        /// <returns>
        /// Una colección enumerable de <see cref="MedicalRecordDto"/> con todos los registros médicos disponibles.
        /// </returns>
        Task<IEnumerable<MedicalRecordDto>> GetAllMedicalRecordsAsync();
    }
}

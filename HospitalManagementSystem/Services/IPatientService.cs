using HospitalManagementSystem.DTOs;

namespace HospitalManagementSystem.Services
{
    /// <summary>
    /// Define la interfaz para las operaciones de servicio relacionadas con los pacientes.
    /// </summary>
    public interface IPatientService
    {
        /// <summary>
        /// Obtiene una lista de todos los pacientes registrados en el sistema.
        /// </summary>
        /// <returns>Tarea que representa la operación asíncrona. Contiene una colección de PatientDto.</returns>
        Task<IEnumerable<PatientDto>> GetAllPatientsAsync();

        /// <summary>
        /// Obtiene los detalles de un paciente específico por su identificador único (GUID).
        /// </summary>
        /// <param name="id">El identificador único (GUID) del paciente.</param>
        /// <returns>Tarea que representa la operación asíncrona. Contiene el PatientDto o null si no se encuentra.</returns>
        Task<PatientDto?> GetPatientByIdAsync(Guid id);

        /// <summary>
        /// Crea un nuevo registro de paciente en la base de datos junto con el usuario asociado.
        /// </summary>
        /// <param name="createPatientDto">Objeto de transferencia de datos que contiene toda la información necesaria para crear el paciente y su cuenta de usuario.</param>
        /// <returns>Tarea que representa la operación asíncrona. Contiene el objeto PatientDto del paciente recién creado.</returns>
        Task<PatientDto> CreatePatientAsync(CreatePatientDto createPatientDto);

        /// <summary>
        /// Actualiza la información de un paciente existente.
        /// </summary>
        /// <param name="id">El identificador único (GUID) del paciente a actualizar.</param>
        /// <param name="updatePatientDto">Objeto de transferencia de datos con la nueva información del paciente.</param>
        /// <returns>Tarea que representa la operación asíncrona. Contiene el PatientDto actualizado o null si no se encuentra.</returns>
        Task<PatientDto?> UpdatePatientAsync(Guid id, CreatePatientDto updatePatientDto);

        /// <summary>
        /// Marca a un paciente como inactivo o lo elimina permanentemente del sistema por su ID.
        /// </summary>
        /// <param name="id">El identificador único (GUID) del paciente a eliminar.</param>
        /// <returns>Tarea que representa la operación asíncrona. Devuelve 'true' si la eliminación/desactivación fue exitosa, 'false' en caso contrario.</returns>
        Task<bool> DeletePatientAsync(Guid id);

        /// <summary>
        /// Obtiene el identificador de usuario (UserId) asociado a un paciente específico.
        /// </summary>
        /// <param name="patientId">El identificador único (GUID) del registro del paciente.</param>
        /// <returns>Tarea que representa la operación asíncrona. Contiene el GUID del usuario asociado.</returns
        Task<Guid> GetPatientUserIdAsync(Guid patientId);

        /// <summary>
        /// Busca pacientes basados en un término de búsqueda (ej. nombre, apellido, documento).
        /// </summary>
        /// <param name="searchTerm">El término o frase a buscar.</param>
        /// <returns>Tarea que representa la operación asíncrona. Contiene una colección de PatientDto que coinciden con el término.</returns>
        Task<IEnumerable<PatientDto>> SearchPatientsAsync(string searchTerm);
    }


}

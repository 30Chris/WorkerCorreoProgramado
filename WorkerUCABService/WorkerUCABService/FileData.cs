using System.Data.SqlClient;
using WorkerUCABService.Procesos;


namespace WorkerUCABService
{
    public class FileData : IFileData
    {

        #region Clases
        Logs l = new Logs();
        #endregion


        private readonly Correo _correo;
       

        public FileData(Correo correo)
        {
            _correo = correo ?? throw new ArgumentNullException(nameof(correo));
          
        }

        public async Task QueryDatabase()
        {
            try
            {

                using (var connection = new SqlConnection(Conexion.CadenaSQL))
                {
                    await connection.OpenAsync();
                    string query = "SELECT id FROM FechasProgramadas";



                    using (var command = new SqlCommand(query, connection))
                    {
                        using (var reader = await command.ExecuteReaderAsync())
                        {
                            while (await reader.ReadAsync())
                            {
                                int Id = reader.GetInt32(0);

                                if (Id > 0) // O cualquier otra condición que consideres válida para Id
                                {
                                    await ProcessCourseData(Id);
                                }
                            }
                        }
                    }
                }


            }
            catch (Exception ex)
            {
                l.ErrorLog(this, ex);
            }

        }

        private async Task ProcessCourseData(int Id)
        {
            try
            {

                using (var connection = new SqlConnection(Conexion.CadenaSQL))
                {
                    await connection.OpenAsync();
                    string subQuery = "SELECT IdDocumento, FechaProg FROM FechasProgramadas WHERE id = @id";

                    using (var subCommand = new SqlCommand(subQuery, connection))
                    {
                        subCommand.Parameters.AddWithValue("@id", Id);

                        using (var subReader = await subCommand.ExecuteReaderAsync())
                        {
                            while (await subReader.ReadAsync())
                            {
                                string idDocumento = subReader.GetString(0);
                                DateTime fechaProg = subReader.GetDateTime(1);

                                if (fechaProg <= DateTime.Now)
                                {
                                    await ProcessDocumentData(idDocumento);
                                }
                            }
                        }
                    }
                }

            }
            catch (Exception ex)
            {
                l.ErrorLog(this, ex);
            }

           
        }

        private async Task ProcessDocumentData(string idDocumento)
        {
            try
            {

                using (var connection = new SqlConnection(Conexion.CadenaSQL))
                {
                    await connection.OpenAsync();
                    string docQuery = "SELECT IdUsuario, Documento, NombreDocumento FROM Documentos WHERE IdDocumento = @IdDocumento";

                    using (var docCommand = new SqlCommand(docQuery, connection))
                    {
                        docCommand.Parameters.AddWithValue("@IdDocumento", idDocumento);

                        using (var docReader = await docCommand.ExecuteReaderAsync())
                        {
                            while (await docReader.ReadAsync())
                            {
                                string idUsuario = docReader.GetString(0);
                                string documento = docReader.GetString(1);
                                string nombreDocumento = docReader.GetString(2);


                                await _correo.EnviaDocumentoAsync(idUsuario, documento, idDocumento, nombreDocumento + ".pdf");

                                //Elimina la fila correspondiente en la tabla FechasProgramadas
                                await DeleteScheduledDateAsync(idDocumento);

                            }
                        }
                    }
                }

            }
            catch (Exception ex)
            {
                l.ErrorLog(this, ex);
            }

          
        }

        // Método para eliminar la fila que ya fue procesada en la tabla FechasProgramadas
        private async Task DeleteScheduledDateAsync(string idDocumento)
        {
            try
            {

                using (var connection = new SqlConnection(Conexion.CadenaSQL))
                {
                    await connection.OpenAsync();
                    string deleteQuery = "DELETE FROM FechasProgramadas WHERE IdDocumento = @IdDocumento";

                    using (var deleteCommand = new SqlCommand(deleteQuery, connection))
                    {
                        deleteCommand.Parameters.AddWithValue("@IdDocumento", idDocumento);
                        await deleteCommand.ExecuteNonQueryAsync();
                    }
                }
            }
            catch (Exception ex)
            {
                l.ErrorLog(this, ex);
            }

           
        }

    }

}

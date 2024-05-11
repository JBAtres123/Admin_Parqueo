using System;
using System.Data;
using System.Data.SqlClient;
using System.Threading.Tasks;
using Admin_Parqueo.Clases;
using Admin_Parqueo.Estructura;



namespace Admin_Parqueo.Servicios
{
    public class ServiceParqueo
    {
        private readonly string _connectionString;
        private readonly ListaEnlazadaSimple parqueoCaballeros;

        public ServiceParqueo(string connectionString)
        {
            _connectionString = connectionString;
            parqueoCaballeros = new ListaEnlazadaSimple();
        }

        public async Task<bool> ManejarEmergencia(int vehiculoID)
        {
            using (var connection = new SqlConnection(_connectionString))
            {
                await connection.OpenAsync();
                using (var command = new SqlCommand("sp_EmergenciaParqueo", connection))
                {
                    command.CommandType = CommandType.StoredProcedure;
                    command.Parameters.AddWithValue("@VehiculoID", vehiculoID);

                    try
                    {
                        await command.ExecuteNonQueryAsync();
                        return true; 
                    }
                    catch (SqlException ex)
                    {
                        Console.WriteLine("Error SQL: " + ex.Message);
                        return false; 
                    }
                }
            }
        }
        public async Task ActualizarRegistroParqueo(RegistroParqueo registro)
        {
            using (var connection = new SqlConnection(_connectionString))
            {
                await connection.OpenAsync();
                using (var command = new SqlCommand(@"
            UPDATE RegistrosParqueo
            SET HoraSalida = @HoraSalida, Estado = @Estado
            WHERE RegistroID = @RegistroID;
        ", connection))
                {
                    command.Parameters.AddWithValue("@HoraSalida", registro.HoraSalida);
                    command.Parameters.AddWithValue("@Estado", registro.Estado);
                    command.Parameters.AddWithValue("@RegistroID", registro.RegistroID);

                    await command.ExecuteNonQueryAsync();
                }
            }
        }
        // En ServiceParqueo.cs

        public async Task<ListaEnlazadaSimple> ObtenerRegistrosParqueoGeneroFemenino()
        {
            ListaEnlazadaSimple registrosFemeninos = new ListaEnlazadaSimple();
            using (var connection = new SqlConnection(_connectionString))
            {
                await connection.OpenAsync();
                using (var command = new SqlCommand(@"
            SELECT *
            FROM RegistrosParqueo
            WHERE TipoGenero = 'F' AND Estado = 'DENTRO'
            ORDER BY HoraEntrada ASC", connection))
                {
                    using (var reader = await command.ExecuteReaderAsync())
                    {
                        while (await reader.ReadAsync())
                        {
                            registrosFemeninos.AgregarAlFinal(new RegistroParqueo
                            {
                                RegistroID = reader.GetInt32(reader.GetOrdinal("RegistroID")),
                                VehiculoID = reader.GetInt32(reader.GetOrdinal("VehiculoID")),
                                HoraEntrada = reader.GetDateTime(reader.GetOrdinal("HoraEntrada")),
                                HoraSalida = reader.IsDBNull(reader.GetOrdinal("HoraSalida")) ? null : (DateTime?)reader.GetDateTime(reader.GetOrdinal("HoraSalida")),
                                Emergencia = reader.GetBoolean(reader.GetOrdinal("Emergencia")),
                                TipoGenero = reader.GetString(reader.GetOrdinal("TipoGenero")),
                                Estado = reader.GetString(reader.GetOrdinal("Estado"))
                            });
                        }
                    }
                }
            }
            return registrosFemeninos;
        }


        public async Task<Vehiculo> SacarVehiculoDeParqueo()
        {
            if (!parqueoCaballeros.ListaVacia())
            {
                Vehiculo vehiculo = (Vehiculo)parqueoCaballeros.EliminarDelFinal();
                if (vehiculo != null)
                {
                    using (SqlConnection connection = new SqlConnection(_connectionString))
                    {
                        await connection.OpenAsync();
                        using (SqlCommand command = connection.CreateCommand())
                        {
                            command.CommandText = @"
                        UPDATE RegistrosParqueo
                        SET Estado = 'AFUERA', HoraSalida = @HoraSalida
                        WHERE VehiculoID = @VehiculoID AND Estado = 'DENTRO';
                    ";

                            command.Parameters.AddWithValue("@VehiculoID", vehiculo.VehiculoID);
                            command.Parameters.AddWithValue("@HoraSalida", DateTime.Now);

                            int affected = await command.ExecuteNonQueryAsync();
                            if (affected > 0)
                            {
                                return vehiculo;
                            }
                        }
                    }
                }
            }
            return null;
        }

        public async Task<Vehiculo> SacarVehiculoEmergencia(int vehiculoID)
{
   
    bool vehiculoEnParqueoCaballeros = await ServiceParqueo.ExisteVehiculoEnParqueoCaballeros(vehiculoID);

    if (vehiculoEnParqueoCaballeros)
    {
        using (SqlConnection connection = new SqlConnection(_connectionString))
        {
            await connection.OpenAsync();
            using (SqlCommand command = connection.CreateCommand())
            {
                command.CommandText = @"
                    UPDATE RegistrosParqueo
                    SET Estado = 'AFUERA', HoraSalida = @HoraSalida
                    WHERE VehiculoID = @VehiculoID AND Estado = 'DENTRO';
                ";

                command.Parameters.AddWithValue("@VehiculoID", vehiculoID);
                command.Parameters.AddWithValue("@HoraSalida", DateTime.Now);

                int affected = await command.ExecuteNonQueryAsync();
                if (affected > 0)
                {
                  
                    Vehiculo vehiculo = await ServiceParqueo.ObtenerVehiculoPorID(vehiculoID);
                    return vehiculo;
                }
            }
        }
    }

    return null;
}

        private static Task<Vehiculo> ObtenerVehiculoPorID(int vehiculoID)
        {
            throw new NotImplementedException();
        }

        private static Task<bool> ExisteVehiculoEnParqueoCaballeros(int vehiculoID)
        {
            throw new NotImplementedException();
        }

        public async Task<ListaEnlazadaSimple> ObtenerRegistrosParqueoGeneroMasculino()
        {
            ListaEnlazadaSimple registrosMasculinos = new ListaEnlazadaSimple();

            using (var connection = new SqlConnection(_connectionString))
            {
                await connection.OpenAsync();
                using (var command = new SqlCommand(@"
            SELECT *
            FROM RegistrosParqueo
            WHERE TipoGenero = 'M' AND Estado = 'DENTRO'
            ORDER BY HoraEntrada ASC", connection))
                {
                    using (var reader = await command.ExecuteReaderAsync())
                    {
                        while (await reader.ReadAsync())
                        {
                            registrosMasculinos.AgregarAlFinal(new RegistroParqueo
                            {
                                RegistroID = reader.GetInt32(reader.GetOrdinal("RegistroID")),
                                VehiculoID = reader.GetInt32(reader.GetOrdinal("VehiculoID")),
                                HoraEntrada = reader.GetDateTime(reader.GetOrdinal("HoraEntrada")),
                                HoraSalida = reader.IsDBNull(reader.GetOrdinal("HoraSalida")) ? null : reader.GetDateTime(reader.GetOrdinal("HoraSalida")),
                                Emergencia = reader.GetBoolean(reader.GetOrdinal("Emergencia")),
                                TipoGenero = reader.GetString(reader.GetOrdinal("TipoGenero")),
                                Estado = reader.GetString(reader.GetOrdinal("Estado"))
                            });
                        }
                    }
                }
            }
            return registrosMasculinos;
        }
        public async Task<RegistroParqueo> ObtenerUltimoVehiculoDentro()
        {
            using (var connection = new SqlConnection(_connectionString))
            {
                await connection.OpenAsync();
                using (var command = new SqlCommand(@"
            SELECT TOP 1 *
            FROM RegistrosParqueo
            WHERE Estado = 'DENTRO' AND TipoGenero = 'M'
            ORDER BY HoraEntrada DESC", connection))
                {
                    using (var reader = await command.ExecuteReaderAsync())
                    {
                        if (await reader.ReadAsync())
                        {
                            return new RegistroParqueo
                            {
                                RegistroID = reader.GetInt32(reader.GetOrdinal("RegistroID")),
                                VehiculoID = reader.GetInt32(reader.GetOrdinal("VehiculoID")),
                                HoraEntrada = reader.GetDateTime(reader.GetOrdinal("HoraEntrada")),
                                HoraSalida = reader.IsDBNull(reader.GetOrdinal("HoraSalida")) ? null : reader.GetDateTime(reader.GetOrdinal("HoraSalida")),
                                Emergencia = reader.GetBoolean(reader.GetOrdinal("Emergencia")),
                                TipoGenero = reader.GetString(reader.GetOrdinal("TipoGenero")),
                                Estado = reader.GetString(reader.GetOrdinal("Estado"))
                            };
                        }
                    }
                }
            }
            return null;
        }

        public async Task<bool> ActualizarEstadoVehiculo(int registroID, string nuevoEstado)
        {
            using (var connection = new SqlConnection(_connectionString))
            {
                await connection.OpenAsync();
                using (var command = new SqlCommand(@"
            UPDATE RegistrosParqueo
            SET Estado = @NuevoEstado, HoraSalida = @HoraSalida
            WHERE RegistroID = @RegistroID", connection))
                {
                    command.Parameters.AddWithValue("@NuevoEstado", nuevoEstado);
                    command.Parameters.AddWithValue("@HoraSalida", DateTime.Now);
                    command.Parameters.AddWithValue("@RegistroID", registroID);

                    int affected = await command.ExecuteNonQueryAsync();
                    return affected > 0;
                }
            }
        }

        public async Task<bool> ExisteVehiculo(int vehiculoID)
        {
            bool existe = false;

            using (SqlConnection connection = new SqlConnection(_connectionString))
            {
                await connection.OpenAsync();

                using (SqlCommand command = connection.CreateCommand())
                {
                    command.CommandText = "SELECT COUNT(*) FROM Vehiculos WHERE VehiculoID = @VehiculoID";
                    command.Parameters.AddWithValue("@VehiculoID", vehiculoID);

                    int count = (int)await command.ExecuteScalarAsync();

                    existe = count > 0;
                }
            }

            return existe;
        }

        public async Task<int> AgregarRegistroParqueo(RegistroParqueo registro)
        {
            using (var connection = new SqlConnection(_connectionString))
            {
                await connection.OpenAsync();
                using (var command = new SqlCommand(@"
            INSERT INTO RegistrosParqueo (VehiculoID, HoraEntrada, TipoGenero, Estado, Emergencia)
            VALUES (@VehiculoID, @HoraEntrada, @TipoGenero, @Estado, @Emergencia);
            SELECT SCOPE_IDENTITY();", connection))
                {
                    command.Parameters.AddWithValue("@VehiculoID", registro.VehiculoID);
                    command.Parameters.AddWithValue("@HoraEntrada", registro.HoraEntrada);
                    command.Parameters.AddWithValue("@TipoGenero", registro.TipoGenero);
                    command.Parameters.AddWithValue("@Estado", registro.Estado);
                    command.Parameters.AddWithValue("@Emergencia", registro.Emergencia);

                    // Ejecutar y obtener el ID del registro insertado
                    object result = await command.ExecuteScalarAsync();
                    if (result != null)
                    {
                        return Convert.ToInt32(result);
                    }
                    throw new Exception("No se pudo insertar el registro de parqueo.");
                }
            }
        }

        public async Task<ListaEnlazadaSimple> ObtenerRegistrosOrdenadosPorHoraEntrada()
        {
            ListaEnlazadaSimple registrosOrdenados = new ListaEnlazadaSimple();
            using (var connection = new SqlConnection(_connectionString))
            {
                await connection.OpenAsync();
                using (var command = new SqlCommand(@"
            SELECT *
            FROM RegistrosParqueo
            WHERE Estado = 'DENTRO'
            ORDER BY HoraEntrada ASC", connection))
                {
                    using (var reader = await command.ExecuteReaderAsync())
                    {
                        while (await reader.ReadAsync())
                        {
                            registrosOrdenados.AgregarAlFinal(new RegistroParqueo
                            {
                                RegistroID = reader.GetInt32(reader.GetOrdinal("RegistroID")),
                                VehiculoID = reader.GetInt32(reader.GetOrdinal("VehiculoID")),
                                HoraEntrada = reader.GetDateTime(reader.GetOrdinal("HoraEntrada")),
                                HoraSalida = reader.IsDBNull(reader.GetOrdinal("HoraSalida")) ? null : reader.GetDateTime(reader.GetOrdinal("HoraSalida")),
                                Emergencia = reader.GetBoolean(reader.GetOrdinal("Emergencia")),
                                TipoGenero = reader.GetString(reader.GetOrdinal("TipoGenero")),
                                Estado = reader.GetString(reader.GetOrdinal("Estado"))
                            });
                        }
                    }
                }
            }
            return registrosOrdenados;
        }



        public async Task<ListaEnlazadaSimple> ObtenerPrimerosRegistros(string estado, int count)
        {
            ListaEnlazadaSimple registros = new ListaEnlazadaSimple();
            using (var connection = new SqlConnection(_connectionString))
            {
                await connection.OpenAsync();
                using (var command = new SqlCommand($@"
                    SELECT TOP {count} *
                    FROM RegistrosParqueo
                    WHERE TipoGenero = 'M' AND Estado = @Estado
                    ORDER BY HoraEntrada ASC", connection))
                {
                    command.Parameters.AddWithValue("@Estado", estado);
                    using (var reader = await command.ExecuteReaderAsync())
                    {
                        while (await reader.ReadAsync())
                        {
                            registros.AgregarAlFinal(new RegistroParqueo
                            {
                                RegistroID = reader.GetInt32(reader.GetOrdinal("RegistroID")),
                                VehiculoID = reader.GetInt32(reader.GetOrdinal("VehiculoID")),
                                HoraEntrada = reader.GetDateTime(reader.GetOrdinal("HoraEntrada")),
                                HoraSalida = reader.IsDBNull(reader.GetOrdinal("HoraSalida")) ? null : (DateTime?)reader.GetDateTime(reader.GetOrdinal("HoraSalida")),
                                Emergencia = reader.GetBoolean(reader.GetOrdinal("Emergencia")),
                                TipoGenero = reader.GetString(reader.GetOrdinal("TipoGenero")),
                                Estado = reader.GetString(reader.GetOrdinal("Estado"))
                            });
                        }
                    }
                }
            }
            return registros;
        }


        public async Task<ListaEnlazadaSimple> ObtenerUltimosRegistros(string estado, int count)
        {
            ListaEnlazadaSimple registros = new ListaEnlazadaSimple();
            using (var connection = new SqlConnection(_connectionString))
            {
                await connection.OpenAsync();
                using (var command = new SqlCommand($@"
                    SELECT TOP {count} *
                    FROM RegistrosParqueo
                    WHERE TipoGenero = 'M' AND Estado = @Estado
                    ORDER BY HoraEntrada DESC", connection))
                {
                    command.Parameters.AddWithValue("@Estado", estado);
                    using (var reader = await command.ExecuteReaderAsync())
                    {
                        while (await reader.ReadAsync())
                        {
                            registros.AgregarAlFinal(new RegistroParqueo
                            {
                                RegistroID = reader.GetInt32(reader.GetOrdinal("RegistroID")),
                                VehiculoID = reader.GetInt32(reader.GetOrdinal("VehiculoID")),
                                HoraEntrada = reader.GetDateTime(reader.GetOrdinal("HoraEntrada")),
                                HoraSalida = reader.IsDBNull(reader.GetOrdinal("HoraSalida")) ? null : (DateTime?)reader.GetDateTime(reader.GetOrdinal("HoraSalida")),
                                Emergencia = reader.GetBoolean(reader.GetOrdinal("Emergencia")),
                                TipoGenero = reader.GetString(reader.GetOrdinal("TipoGenero")),
                                Estado = reader.GetString(reader.GetOrdinal("Estado"))
                            });
                        }
                    }
                }
            }
            return registros;
        }
        public async Task<ListaEnlazadaSimple> ObtenerRegistrosParqueoGeneroMasculinoAFUERA()
        {
            ListaEnlazadaSimple registros = new ListaEnlazadaSimple();

            using (SqlConnection connection = new SqlConnection(_connectionString))
            {
                await connection.OpenAsync();

                using (SqlCommand command = connection.CreateCommand())
                {
                    command.CommandText = @"
                SELECT RegistroID, VehiculoID, HoraEntrada, HoraSalida, Emergencia, TipoGenero, Estado
                FROM RegistrosParqueo
                WHERE TipoGenero = 'M' AND Estado = 'AFUERA';
            ";

                    using (SqlDataReader reader = await command.ExecuteReaderAsync())
                    {
                        while (await reader.ReadAsync())
                        {
                            RegistroParqueo registro = new RegistroParqueo
                            {
                                RegistroID = reader.GetInt32(0),
                                VehiculoID = reader.GetInt32(1),
                                HoraEntrada = reader.GetDateTime(2),
                                HoraSalida = reader.IsDBNull(3) ? (DateTime?)null : reader.GetDateTime(3),
                                Emergencia = reader.GetBoolean(4),
                                TipoGenero = reader.GetString(5),
                                Estado = reader.GetString(6)
                            };

                            registros.AgregarAlFinal(registro);
                        }
                    }
                }
            }

            return registros;
        }


        public async Task AddUsuario(string nombre, string apellido, char genero, string telefono)
        {
            using (SqlConnection connection = new SqlConnection(_connectionString))
            {
                await connection.OpenAsync();
                using (SqlCommand command = connection.CreateCommand())
                {
                    command.CommandText = @"
                        INSERT INTO Usuarios (Nombre, Apellido, Genero, Telefono)
                        VALUES (@Nombre, @Apellido, @Genero, @Telefono);
                    ";

                    command.Parameters.AddWithValue("@Nombre", nombre);
                    command.Parameters.AddWithValue("@Apellido", apellido);
                    command.Parameters.AddWithValue("@Genero", genero);
                    command.Parameters.AddWithValue("@Telefono", telefono);

                    await command.ExecuteNonQueryAsync();
                }
            }
        }
        public async Task<bool> VerificarUsuarioExistente(int usuarioId)
        {
            using (var connection = new SqlConnection(_connectionString))
            {
                await connection.OpenAsync();
                var command = new SqlCommand("SELECT COUNT(*) FROM Usuarios WHERE UsuarioID = @UsuarioID", connection);
                command.Parameters.AddWithValue("@UsuarioID", usuarioId);

                var count = (int)await command.ExecuteScalarAsync();
                return count > 0;
            }
        }
        public async Task AddVehiculo(Vehiculo vehiculo)
        {
            using (var connection = new SqlConnection(_connectionString))
            {
                await connection.OpenAsync();
                var command = new SqlCommand("INSERT INTO Vehiculos (UsuarioID, Placa, TipoVehiculo, Modelo, Color) VALUES (@UsuarioID, @Placa, @TipoVehiculo, @Modelo, @Color)", connection);
                command.Parameters.AddWithValue("@UsuarioID", vehiculo.UsuarioID);
                command.Parameters.AddWithValue("@Placa", vehiculo.Placa);
                command.Parameters.AddWithValue("@TipoVehiculo", vehiculo.TipoVehiculo);
                command.Parameters.AddWithValue("@Modelo", vehiculo.Modelo);
                command.Parameters.AddWithValue("@Color", vehiculo.Color);

                await command.ExecuteNonQueryAsync();
            }
        }

        public void AgregarAlParqueoCaballeros(Vehiculo vehiculo)
        {
            parqueoCaballeros.AgregarAlInicio(vehiculo);
        }
        private async Task AddVehiculoToDatabase(Vehiculo vehiculo)
        {
            using (SqlConnection connection = new SqlConnection(_connectionString))
            {
                await connection.OpenAsync();
                using (SqlCommand command = connection.CreateCommand())
                {
                    command.CommandText = @"
                        INSERT INTO Vehiculos (UsuarioID, Placa, TipoVehiculo, Modelo, Color)
                        VALUES (@UsuarioID, @Placa, @TipoVehiculo, @Modelo, @Color);
                    ";

                    command.Parameters.AddWithValue("@UsuarioID", vehiculo.UsuarioID);
                    command.Parameters.AddWithValue("@Placa", vehiculo.Placa);
                    command.Parameters.AddWithValue("@TipoVehiculo", vehiculo.TipoVehiculo);
                    command.Parameters.AddWithValue("@Modelo", vehiculo.Modelo);
                    command.Parameters.AddWithValue("@Color", vehiculo.Color);

                    await command.ExecuteNonQueryAsync();
                }
            }
        }
        public Vehiculo SacarDelParqueoCaballeros()
        {
            if (!parqueoCaballeros.ListaVacia())
            {
                Vehiculo vehiculo = (Vehiculo)parqueoCaballeros.PrimerNodo.Informacion;
                parqueoCaballeros.Eliminar(vehiculo);
                return vehiculo;
            }
            return null;
        }

        public ListaEnlazadaSimple ObtenerVehiculosParqueoCaballeros()
        {
            return parqueoCaballeros;
        }
    }
}
        









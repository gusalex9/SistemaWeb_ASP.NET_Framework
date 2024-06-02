using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data.SqlClient;
using dominio;
using System.Diagnostics.Contracts;
using System.Net;
using System.Collections;

namespace negocio
{
    public class PokemonNegocio
    {
        public List<Pokemons> listar()
        {
            List<Pokemons> lista = new List<Pokemons >();
            AccesoDatos datos = new AccesoDatos();

            try
            {
                datos.setearConsulta("SELECT P.Id, Numero, Nombre, P.Descripcion, UrlImagen, E.Descripcion Tipo, D.Descripcion Debilidad, P.IdTipo, P.IdDebilidad FROM POKEMONS P, ELEMENTOS E, ELEMENTOS D WHERE E.Id = P.IdTipo AND D.ID = P.IdDebilidad AND P.Activo = 1");
                datos.ejecutarLectura();

                while (datos.Lector.Read())
                {
                    Pokemons aux = new Pokemons();
                    aux.Id = (int)datos.Lector["Id"];
                    aux.Numero = (int)datos.Lector["Numero"];
                    aux.Nombre = (string)datos.Lector["Nombre"];
                    aux.Descripcion = (string)datos.Lector["Descripcion"];

                    //Validacion de la lectura null de la columna UrlImagen
                    if (!(datos.Lector["UrlImagen"] is DBNull))
                        aux.UrlImagen = (string)datos.Lector["UrlImagen"];

                    //Como la propiedad Tipo es una clase primero hay que instanciar
                    aux.Tipo = new Elemento();
                    aux.Tipo.Id = (int)datos.Lector["IdTipo"]; //Anadiendo el Id de tipo
                    aux.Tipo.Descripcion = (string)datos.Lector["Tipo"];
                    
                    aux.Debilidad = new Elemento(); //Anadiendo el Id de la debilidad
                    aux.Debilidad.Id = (int)datos.Lector["IdDebilidad"];
                    aux.Debilidad.Descripcion = (string)datos.Lector["Debilidad"];
                    lista.Add(aux);
                }
                return lista;
            }
            catch (Exception ex)
            {
                throw ex;
            }
            finally 
            {
                datos.cerrarConexion();
            }
        }
        public void agregar(Pokemons nuevoPokemon)
        {
            AccesoDatos datos = new AccesoDatos();
            try
            {
                datos.setearConsulta($"Insert into POKEMONS(Numero, Nombre, Descripcion, UrlImagen, Activo, IdTipo, IdDebilidad )values({nuevoPokemon.Numero}, '{nuevoPokemon.Nombre}', '{nuevoPokemon.Descripcion}', @urlImagen, 1, @IdTipo, @idDebilidad)");
                datos.setearParametro("@idTipo", nuevoPokemon.Tipo.Id);
                datos.setearParametro("@idDebilidad", nuevoPokemon.Debilidad.Id);
                datos.setearParametro("@urlImagen", nuevoPokemon.UrlImagen);
                datos.ejecutarAccion();
            }
            catch (Exception ex)
            {

                throw ex;
            }
            finally 
            { 
                datos.cerrarConexion();
            }
        }

        public void modificar(Pokemons pokemon)
        {
            AccesoDatos datos= new AccesoDatos();
            try
            {
                datos.setearConsulta("update POKEMONS set Numero = @numero, Nombre= @nombre, Descripcion= @desc, UrlImagen=@imagen, IdTipo = @tipo, IdDebilidad = @debilidad where id = @id");
                datos.setearParametro("@numero", pokemon.Numero);
                datos.setearParametro("@nombre", pokemon.Nombre);
                datos.setearParametro("@desc", pokemon.Descripcion);
                datos.setearParametro("@imagen", pokemon.UrlImagen);
                datos.setearParametro("@tipo", pokemon.Tipo.Id);
                datos.setearParametro("@debilidad", pokemon.Debilidad.Id);
                datos.setearParametro("@id", pokemon.Id);

                datos.ejecutarAccion();
            }
            catch (Exception ex)
            {
                throw ex;
            }
            finally {datos.cerrarConexion();}
        }

        public void eliminarFisico(int IdPokemon)
        {
            AccesoDatos datos = new AccesoDatos();
            try
            {
                datos.setearConsulta("delete from POKEMONS where id = @id");
                datos.setearParametro("@id", IdPokemon);
                datos.ejecutarAccion();
            }
            catch (Exception ex)
            {

                throw ex;
            }finally { datos.cerrarConexion();}
        }

        public void eliminarLogico(int idPokemon)
        {
            AccesoDatos datos = new AccesoDatos();
            try
            {
                datos.setearConsulta("update POKEMONS set Activo = 0 where id= @id");
                datos.setearParametro("@id", idPokemon);
                datos.ejecutarAccion();
            }
            catch (Exception ex)
            {

                throw ex;
            }
            finally { datos.cerrarConexion();}
        }

        public List<Pokemons> filtrar(string campo, string criterio, string filtro)
        {
            List<Pokemons > list = new List<Pokemons >();
            AccesoDatos data = new AccesoDatos();
            try
            {
                //La consulta tiene que ser la misma que la del metodo listar con un espacion 
                string consulta = "SELECT P.Id, Numero, Nombre, P.Descripcion, UrlImagen, E.Descripcion Tipo, D.Descripcion Debilidad, P.IdTipo, P.IdDebilidad FROM POKEMONS P, ELEMENTOS E, ELEMENTOS D WHERE E.Id = P.IdTipo AND D.ID = P.IdDebilidad AND P.Activo = 1 AND ";

                switch (campo)
                {
                    case "Numero":
                        switch (criterio)
                        {
                            case "Mayor a":
                                consulta += "Numero > " + filtro;
                                break;
                            case "Menor a":
                                consulta += "Numero < " + filtro;
                                break;
                            default:
                                consulta += "Numero = " + filtro;
                                break;
                        }
                        break;

                    case "Nombre":
                        switch (criterio)
                        {
                            case "Comienza con":
                                consulta += $"Nombre like '{filtro}%'";
                                break;
                            case "Termina con":
                                consulta += $"Nombre like '%{filtro}'";
                                break;
                            default:
                                consulta += $"Nombre like '%{filtro}%'";
                                break;
                        }
                        break;

                    default:
                        switch (criterio)
                        {
                            case "Comienza con":
                                consulta += $"P.Descripcion like '{filtro}%'";
                                break;

                            case "Termina con":
                                consulta += $"P.Descripcion like '%{filtro}'";
                                break;

                            default:
                                consulta += $"P.Descripcion like '%{filtro}%'";
                                break;
                        }
                        break;
                }

                data.setearConsulta( consulta );
                data.ejecutarLectura();

                while (data.Lector.Read())
                {
                    Pokemons aux = new Pokemons();
                    aux.Id = (int)data.Lector["Id"];
                    aux.Numero = (int)data.Lector["Numero"];
                    aux.Nombre = (string)data.Lector["Nombre"];
                    aux.Descripcion = (string)data.Lector["Descripcion"];

                    if (!(data.Lector["UrlImagen"] is DBNull))
                        aux.UrlImagen = (string)data.Lector["UrlImagen"];

                    aux.Tipo = new Elemento();
                    aux.Tipo.Id = (int)data.Lector["IdTipo"]; 
                    aux.Tipo.Descripcion = (string)data.Lector["Tipo"];

                    aux.Debilidad = new Elemento(); 
                    aux.Debilidad.Id = (int)data.Lector["IdDebilidad"];
                    aux.Debilidad.Descripcion = (string)data.Lector["Debilidad"];
                    list.Add(aux);
                }

                return list;
            }
            catch (Exception ex)
            {
                throw ex;
            }finally
            {
                data.cerrarConexion();
            }
        }
    }
}

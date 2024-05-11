using System;
using System.Collections;
using System.Collections.Generic;

namespace Admin_Parqueo.Estructura
{
    public class ListaEnlazadaSimple : IEnumerable<Nodo>
    {
        public Nodo PrimerNodo { get; private set; }
        public Nodo UltimoNodo { get; private set; }
        public int Tamanio { get; private set; }

        public ListaEnlazadaSimple()
        {
            PrimerNodo = null;
            UltimoNodo = null;
            Tamanio = 0;
        }

        public void AgregarAlFinal(object informacion)
        {
            Nodo nuevoNodo = new Nodo(informacion);
            if (PrimerNodo == null)
            {
                PrimerNodo = nuevoNodo;
                UltimoNodo = nuevoNodo;
            }
            else
            {
                UltimoNodo.Referencia = nuevoNodo;
                UltimoNodo = nuevoNodo;
            }
            Tamanio++;
        }

        public void AgregarAlInicio(object informacion)
        {
            Nodo nuevoNodo = new Nodo(informacion);
            if (PrimerNodo == null)
            {
                PrimerNodo = nuevoNodo;
                UltimoNodo = nuevoNodo;
            }
            else
            {
                nuevoNodo.Referencia = PrimerNodo;
                PrimerNodo = nuevoNodo;
            }
            Tamanio++;
        }

        public Nodo Buscar(object informacion)
        {
            Nodo actual = PrimerNodo;
            while (actual != null)
            {
                if (actual.Informacion.Equals(informacion))
                {
                    return actual;
                }
                actual = actual.Referencia;
            }
            return null; // No encontrado
        }

        public bool ListaVacia()
        {
            return PrimerNodo == null;
        }

        public bool Eliminar(object informacion)
        {
            Nodo actual = PrimerNodo;
            Nodo anterior = null;

            while (actual != null)
            {
                if (actual.Informacion.Equals(informacion))
                {
                    if (anterior == null)
                    {
                        PrimerNodo = actual.Referencia;
                        if (PrimerNodo == null)
                            UltimoNodo = null;
                    }
                    else
                    {
                        anterior.Referencia = actual.Referencia;
                        if (anterior.Referencia == null)
                            UltimoNodo = anterior;
                    }
                    Tamanio--;
                    return true;
                }
                anterior = actual;
                actual = actual.Referencia;
            }

            return false; // No encontrado
        }

        public object EliminarDelFinal()
        {
            if (ListaVacia())
            {
                return null;
            }

            if (PrimerNodo == UltimoNodo)
            {
                object informacion = PrimerNodo.Informacion;
                PrimerNodo = UltimoNodo = null;
                Tamanio--;
                return informacion;
            }

            Nodo actual = PrimerNodo;
            while (actual.Referencia != UltimoNodo)
            {
                actual = actual.Referencia;
            }

            object info = UltimoNodo.Informacion;
            UltimoNodo = actual;
            UltimoNodo.Referencia = null;
            Tamanio--;
            return info;
        }


        public IEnumerator<Nodo> GetEnumerator()
        {
            Nodo actual = PrimerNodo;
            while (actual != null)
            {
                yield return actual;
                actual = actual.Referencia;
            }
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}
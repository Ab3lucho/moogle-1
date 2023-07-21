using System.Collections.Generic;
using System;
using System.IO;
using System.Diagnostics;
using System.Linq;

namespace Busqueda
{
    class busqueda
    {
        // Importado de ManejoDeArchivos//
        public string[] ArchivosTxt { get; set; }
        public Dictionary<string, string[]> PalabrasUnicas = new Dictionary<string, string[]>();
        public Dictionary<string, string[]> NombresvsPalabras = new Dictionary<string, string[]>();
        //Importado de TF_IDF//
        public Dictionary<string, Dictionary<string, double>> DiccionarioTF_IDF = new Dictionary<string, Dictionary<string, double>>();
        //Declaracion de diccionarios Query//
        public Dictionary<string, double> Query_TF = new Dictionary<string, double>();
        public Dictionary<string, double> Query_IDF = new Dictionary<string, double>();
        public Dictionary<string, double> Query_TF_IDF = new Dictionary<string, double>();

        public Dictionary<double, string> Results = new Dictionary<double, string>();


        //Diccionarios de la similitud//
        public Dictionary<string, double> Similitud_Coseno;
        public Dictionary<string, double> Similitud_CosenoOrdenado;
        //Results
        public Dictionary<string, string> Snippet;

        // Declaracion de variables Query//
        public string query { get; set; }
        public string[] QueryTokenizada { get; set; }


        //Constructor//

        public busqueda(string[] archivostxt, Dictionary<string, string[]> palabrasunicas, Dictionary<string, string[]> nombresvspalabras, Dictionary<string, Dictionary<string, double>> diccionarioTF_IDF, string Query)
        {
            ArchivosTxt = archivostxt;
            PalabrasUnicas = palabrasunicas;
            NombresvsPalabras = nombresvspalabras;
            query = Query;
            DiccionarioTF_IDF = diccionarioTF_IDF;
            
            Dictionary<double, string> Results;




        }

        //Tokenizar la query y convertir en minusculas//

        public string[] TokenizarQuery()
        {
            char[] delimitadores = { ' ', ',', '.', ':', '¿', '?', '!', '*', '/', '"', '#', ')', '(', };
            QueryTokenizada = this.query.Split(delimitadores);
            for (int i = 0; i < QueryTokenizada.Length; i++)
            {
                QueryTokenizada[i] = QueryTokenizada[i].ToLower();
            }
            return QueryTokenizada;
        }

        //Contador de documentos donde existe la palabra//

        public double ContadorPalabra(string Palabra, Dictionary<string, string[]> NombrevsPalabras, string[] ArchivosTxt)
        {
            double DocumentosDodeExiste = 0;

            for (int i = 0; i < ArchivosTxt.Length; i++)
            {
                string[] Documento = NombrevsPalabras[ArchivosTxt[i]];

                for (int x = 0; x < Documento.Length; x++)
                {

                    if (Documento[x] == Palabra)
                    {

                        DocumentosDodeExiste++;
                        break;

                    }

                }

            }
            return DocumentosDodeExiste;
        }

        //Metodo TF de la query//

        public void TF_Query()
        {
            foreach (var item in QueryTokenizada)
            {
                double contador = 0;
                foreach (var item2 in QueryTokenizada)
                {
                    if (item == item2)
                    {
                        contador++;
                    }
                }
                Query_TF.Add(item, contador / QueryTokenizada.Length);
            }
        }

        //Metodo IDF de la query//  

        public void IDF_Query()
        {
            double cantidad_total_documentos = this.ArchivosTxt.Length;
            for (int i = 0; i < QueryTokenizada.Length; i++)
            {
                //Se calcula el IDF de la palabra y se agrega al diccionario de IDF del query
                double idf = Math.Log(cantidad_total_documentos / ContadorPalabra(QueryTokenizada[i], this.NombresvsPalabras, this.ArchivosTxt));
                if (Query_IDF.ContainsKey(QueryTokenizada[i]) != true)
                {
                    Query_IDF.Add(QueryTokenizada[i], idf);
                }
            }
        }

        //Metodo TF-IDF de la query//

        public void TF_IDF_Query()
        {
            foreach (var item in QueryTokenizada)
            {
                Query_TF_IDF.Add(item, Query_TF[item] * Query_IDF[item]);

            }
        }

        //Motor de busqueda//

        public void Motor_Busqueda()
        {
            TokenizarQuery();
            TF_Query();
            Console.WriteLine("TF de la query Cargado");
            IDF_Query();
            Console.WriteLine("IDF de la query Cargado");
            TF_IDF_Query();
            Console.WriteLine("TF-IDF de la query Cargado");
            Console.WriteLine("Buscando...");

        }

        // Metodo de Diccionario de similitud//

        public void Similitud_TFIDF_Query()
        {
            Similitud_Coseno = new Dictionary<string, double>();

            foreach (var name in this.ArchivosTxt)
            {
                Dictionary<string, double> DiccionarioTFIDF_Query = new Dictionary<string, double>();
                foreach (var queryWord in QueryTokenizada)
                {
                    if (this.DiccionarioTF_IDF[name].ContainsKey(queryWord))
                    {
                        if (DiccionarioTFIDF_Query.ContainsKey(queryWord) == false)
                        {
                            DiccionarioTFIDF_Query.Add(queryWord, DiccionarioTF_IDF[name][queryWord]*Query_TF_IDF[queryWord]);
                        }
                    }
                }
                double sum = 0;
                foreach (var item in DiccionarioTFIDF_Query.Values)
                {
                    sum+=item;
                }
                Similitud_Coseno.Add(name, sum);
            }


            Similitud_CosenoOrdenado = new Dictionary<string, double>();
            foreach (var item in ArchivosTxt)
            {
                double contador = 0;
                double contador2 = 0;
                double contador3 = 0;
                foreach (var item2 in DiccionarioTF_IDF[item].Values)
                {
                    contador2 += Math.Pow(item2, 2);
                }
                foreach (var item3 in Query_TF_IDF.Values)
                {
                    contador3 += Math.Pow(item3, 2);
                }
                contador = Math.Sqrt(contador2) * Math.Sqrt(contador3);
                Similitud_CosenoOrdenado.Add(item, contador);
            }
            foreach (var name in this.ArchivosTxt)
            {
                Similitud_CosenoOrdenado[name] = Similitud_Coseno[name] / Similitud_CosenoOrdenado[name];
            }
            
            // Ordenar Similitud Coseno Ordenado//
            var items = from pair in Similitud_CosenoOrdenado
                orderby pair.Value descending
                select pair;

            foreach (var item in Similitud_CosenoOrdenado.Keys)
            {
                if (Similitud_CosenoOrdenado[item] > 0)
                {
                    if (!this.Results.ContainsKey(Similitud_CosenoOrdenado[item]))
                    {
                        this.Results.Add(Similitud_CosenoOrdenado[item], item);
                    }
                }
            }
        }    
        
        // Motor de resultados //

        public void Motor_Resultados()
        {
            Similitud_TFIDF_Query();
            Console.WriteLine("Similitud TF-IDF de la query Cargado");
            Console.WriteLine("Similitud Coseno de la query Cargado");
            Console.WriteLine("Resultados Cargado");
        }

        
        // Calcular el Snippet con 5 elementos antes y despues del query  //

        public void Snippet_()
        {
            Snippet = new Dictionary<string, string>();
            foreach (var item in Results)
            {
                string[] Documento = NombresvsPalabras[item.Value];
                string[] palabrasunicas = PalabrasUnicas[item.Value];
                string[] Snippet = new string[10];

                // Buscar la posicion de la palabra clave en el documento //

                int posicion = 0;
                for (int i = 0; i < Documento.Length; i++)
                {
                    if (Documento[i] == palabrasunicas[0])
                    {
                        posicion = i;
                        break;
                    }
                }

                // Agregar las 5 palabras antes y despues de la palabra clave //

                int contador = 0;
                int contador2 = 0;
                for (int i = posicion; i < Documento.Length; i++)
                {
                    if (contador < 5)
                    {
                        Snippet[contador] = Documento[i];
                        contador++;
                    }
                    else
                    {
                        break;
                    }
                }
                for (int i = posicion - 1; i > 0; i--)
                {
                    if (contador2 < 5)
                    {
                        Snippet[contador] = Documento[i];
                        contador++;
                        contador2++;
                    }
                    else
                    {
                        break;
                    }
                }

                // Agregar el Snippet al diccionario de Snippets //

                string SnippetFinal = "";
                foreach (var item2 in Snippet)
                {
                    SnippetFinal += item2 + " ";
                }
                this.Snippet.Add(item.Value, SnippetFinal);


            }

        }

        // Motor de  Snippet //

        public void Motor_Snippet()
        {
            
            Snippet_();
            Console.WriteLine("Snippet Cargado");
            Console.WriteLine("Busqueda finalizada");
        }







    }


}
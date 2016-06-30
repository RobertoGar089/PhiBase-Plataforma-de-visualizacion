using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using VDS.RDF;
using VDS.RDF.Query;

namespace appPlantaPatogeno.Controllers
{
    public class HomeController : Controller
    {
        //declaramos las variables para el metodo recursivo
        private int level = 0;
        private string padre = "";
        private int total = 0;
        private string json = "{\"class\": \"go.TreeModel\",\"nodeDataArray\": [";
        private string json2 = "";
        private string finTotal = "]}";
        // GET: Home
        public ActionResult Index()
        {
            //Decclaramos todos los string necesarios para las consultas
            string url = Request.QueryString["interaction"];
            string nodo = Request.QueryString["class_type"];
                        //Cargamos el prefijo para usarlo en todas las consultas
            string prefijos = "PREFIX RO: <http://www.obofoundry.org/ro/ro.owl#>PREFIX SIO: <http://semanticscience.org/resource/>PREFIX EDAM:  <http://edamontology.org/>PREFIX PHIO: <http://linkeddata.systems/ontologies/SemanticPHIBase#>PREFIX PUBMED:  <http://linkedlifedata.com/resource/pubmed/>PREFIX rdfs: <http://www.w3.org/2000/01/rdf-schema#>PREFIX up:  <http://purl.uniprot.org/core/>PREFIX foaf: <http://xmlns.com/foaf/0.1/>PREFIX skos: <http://www.w3.org/2004/02/skos/core#>";
            string inicio = "SELECT DISTINCT ?disn_1 ?label ?rel ?valor WHERE { ?disn_1 ?rel ?valor . ?disn_1 rdfs:label ?label FILTER(( ?disn_1 = <";
            string fin = ">))}";
            Boolean defecto = true;
            //Se establece el endpoint para las consultas
            SparqlRemoteEndpoint endpoint = new SparqlRemoteEndpoint(new Uri("http://linkeddata.systems:8890/sparql"));
            try
            {
                //Creamos la query de la interaction que proviene de la url
                string query = prefijos + inicio + url + fin;
                SparqlResultSet resultQuery1 = endpoint.QueryWithResultSet(query);
                //recorremos los resultados de esta primera query
                if (resultQuery1.Results != null)
                {
                    metodoRecursivo(resultQuery1, padre, level);
                }
                json = json.TrimEnd(',');
                json = json + finTotal;
                ViewData["Nodos"] = json;
                if (nodo!=null) {
                    string query2 = prefijos + inicio + nodo + fin;
                    SparqlResultSet resultQuery2 = endpoint.QueryWithResultSet(query2);
                    if (resultQuery2.Results != null)
                    {
                        metodoNodoCentral(resultQuery2,defecto);
                    }
                    ViewData["Nodo"] = json2;
                }
                if ((nodo == null) && resultQuery1.Results != null)
                {
                    defecto = false;
                    metodoNodoCentral(resultQuery1, defecto);
                    ViewData["Nodo"] = json2;
                }
            }
            catch (RdfQueryException queryEx)
            {
                //There was an error executing the query so handle it here
                Console.WriteLine(queryEx.Message);
            }
            return View();
        }
        private SparqlResultSet metodoNodoCentral(SparqlResultSet n,Boolean d)
        {
            try
            {
                if (d)
                {
                    foreach (var linea in n.Results)
                    {
                        int counter = 0;
                        string disn = "";
                        string label = "";
                        string rel = "";
                        string valor = "";
                        string[] res = linea.ToString().Split(',');
                        foreach (var linea2 in res)
                        {
                            if (!linea2.StartsWith("@"))
                            {
                                if (counter == 0)
                                {
                                    disn = linea2.ToString();
                                }
                                if (counter == 1)
                                {
                                    label = linea2.ToString();
                                }
                                if (counter == 2)
                                {
                                    rel = linea2.ToString();
                                }
                                if (counter == 3)
                                {
                                    valor = linea2.ToString();
                                }
                                counter++;
                            }
                        }

                        string[] prueba = rel.Split('#');
                        if (prueba.Length > 1)
                        {
                            var prueba1 = prueba[1].ToString();
                            if (prueba1 == "label ")
                            {
                                string valorNodo = "";
                                string[] cadena = rel.Split('#');
                                string[] cadena2 = valor.Split('-');
                                string[] cadena3 = cadena2[0].Split('=');
                                if (cadena2.Length > 1)
                                {
                                    valorNodo = cadena3[1] + ":" + cadena2[1];
                                }
                                else
                                {
                                    valorNodo = cadena3[1];
                                }
                                json2 = valorNodo;
                            }
                        }
                    }

                }
                else {
                    foreach (var linea in n.Results)
                    {
                        string prueba1 = "";
                        int counter = 0;
                        string disn = "";
                        string label = "";
                        string rel = "";
                        string valor = "";
                        string[] res = linea.ToString().Split(',');
                        foreach (var linea2 in res)
                        {
                            if (counter == 0)
                            {
                                disn = linea2.ToString();
                            }
                            if (counter == 1)
                            {
                                label = linea2.ToString();
                            }
                            if (counter == 2)
                            {
                                rel = linea2.ToString();
                            }
                            if (counter == 3)
                            {
                                valor = linea2.ToString();
                            }
                            counter++;
                        }
                        string[] prueba = rel.Split('#');
                        prueba1 = prueba[1].ToString();
                        if (prueba1 == "label ")//valor
                        {
                            string valorNodo = "";
                            string[] cadena = rel.Split('#');
                            string[] cadena2 = valor.Split('-');
                            if (cadena2.Length == 3)
                            {
                                string[] i = cadena2[1].Split('[');
                                string[] j = i[1].Split(']');
                                valorNodo = " Interaction:" + j[0];
                            }
                            json2 = valorNodo;
                        }
                    }
                }
                }
            catch (RdfQueryException queryEx)
            {
                //There was an error executing the query so handle it here
                Console.WriteLine(queryEx.Message);
            }
            return n;
        }
        private SparqlResultSet metodoRecursivo(SparqlResultSet n,string padre, int level)
        {
            try
            {
                if(level == 0)
                {
                    total = n.Count;
                }
                level++;
                int count = 0; ;
                if (level == 1)
                {
                    foreach (var linea in n.Results)
                    {
                        string prueba1 = "";
                        if (total > count)
                        {
                            int counter = 0;
                            string disn = "";
                            string label = "";
                            string rel = "";
                            string valor = "";
                            string[] res = linea.ToString().Split(',');
                            foreach (var linea2 in res)
                            {
                                    if (counter == 0)
                                    {
                                        disn = linea2.ToString();
                                    }
                                    if (counter == 1)
                                    {
                                        label = linea2.ToString();
                                    }
                                    if (counter == 2)
                                    {
                                        rel = linea2.ToString();
                                    }
                                    if (counter == 3)
                                    {
                                        valor = linea2.ToString();
                                    }
                                    counter++;
                            }
                            string[] prueba = rel.Split('#');
                            prueba1 = prueba[1].ToString();
                            if ((prueba1 == "has_participant ") || (prueba1 == "has_unique_identifier ") || (prueba1 == "is_manifested_as ") || (prueba1 == "has_unique_identifier "))//relaciones
                            {
                                //Decclaramos todos los string necesarios para las consultas
                                string[] k = valor.Split('=');
                                string url1 = k[1];
                                string url = url1.TrimStart();
                                //Cargamos el prefijo para usarlo en todas las consultas
                                string prefijos = "PREFIX RO: <http://www.obofoundry.org/ro/ro.owl#>PREFIX SIO: <http://semanticscience.org/resource/>PREFIX EDAM:  <http://edamontology.org/>PREFIX PHIO: <http://linkeddata.systems/ontologies/SemanticPHIBase#>PREFIX PUBMED:  <http://linkedlifedata.com/resource/pubmed/>PREFIX rdfs: <http://www.w3.org/2000/01/rdf-schema#>PREFIX up:  <http://purl.uniprot.org/core/>PREFIX foaf: <http://xmlns.com/foaf/0.1/>PREFIX skos: <http://www.w3.org/2004/02/skos/core#>";
                                string inicio = "SELECT DISTINCT ?disn_1 ?label ?rel ?valor WHERE { ?disn_1 ?rel ?valor . ?disn_1 rdfs:label ?label FILTER(( ?disn_1 = <";
                                string fin = ">))}";

                                //Se establece el endpoint para las consultas
                                SparqlRemoteEndpoint endpoint = new SparqlRemoteEndpoint(new Uri("http://linkeddata.systems:8890/sparql"));

                                string query = prefijos + inicio + url + fin;
                                SparqlResultSet resultQuery2 = endpoint.QueryWithResultSet(query);
                                //recorremos los resultados de esta primera query
                                if (resultQuery2.Results != null)
                                {
                                    metodoRecursivo(resultQuery2, padre, level);
                                }
                            }
                            else if (prueba1 == "label ")//valor
                            {
                                string levelColor = "";
                                string valorNodo = "";
                                if (level == 1)
                                {
                                    levelColor = "orange";
                                }
                                else if (level == 2)
                                {
                                    levelColor = "lightblue";
                                }
                                else if (level == 3)
                                {
                                    levelColor = "lightgreen";
                                }
                                else if (level == 4)
                                {
                                    levelColor = "blue";
                                }
                                else if (level == 5)
                                {
                                    levelColor = "purple";
                                }
                                else
                                {
                                    levelColor = "yellow";
                                }
                                string[] cadena = rel.Split('#');
                                string[] cadena2 = valor.Split('-');
                                if (cadena2.Length == 3)
                                {
                                    string[] i = cadena2[1].Split('[');
                                    string[] j = i[1].Split(']');
                                    valorNodo = " Interaction:" + j[0];
                                }
                                string key = "{\"key\":\"";
                                string color = "\" ,\"color\":\"";
                                string texto = "\" ,\"texto\": \"";
                                string fin = "\"} ,";
                                string resultado = key + valorNodo + color + levelColor + texto + fin;
                                json = json + resultado;
                                padre = valorNodo;
                            }
                            count++;
                        }

                    }
                }
                else
                {
                    string levelColor = "";
                    Boolean tipo2 = false;
                    Boolean tipo3 = false;
                    if (level == 1)
                    {
                        levelColor = "orange";
                    }
                    else if (level == 2)
                    {
                        levelColor = "lightblue";
                    }
                    else if (level == 3)
                    {
                        levelColor = "lightgreen";
                    }
                    else if (level == 4)
                    {
                        levelColor = "blue";
                    }
                    else if (level == 5)
                    {
                        levelColor = "purple";
                    }
                    else
                    {
                        levelColor = "yellow";
                    }
                    //primero recorremos el propio nodo para obtener la key
                    foreach (var linea in n.Results)
                    {
                        int counter = 0;
                        string disn = "";
                        string label = "";
                        string rel = "";
                        string valor = "";
                        string[] res = linea.ToString().Split(',');
                        foreach (var linea2 in res)
                        {
                            if (!linea2.StartsWith("@"))
                            {
                                if (counter == 0)
                                {
                                    disn = linea2.ToString();
                                }
                                if (counter == 1)
                                {
                                    label = linea2.ToString();
                                }
                                if (counter == 2)
                                {
                                    rel = linea2.ToString();
                                }
                                if (counter == 3)
                                {
                                    valor = linea2.ToString();
                                }
                                counter++;
                            }
                        }

                        string[] prueba = rel.Split('#');
                        if (prueba.Length > 1)
                        {
                            var prueba1 = prueba[1].ToString();
                            if (prueba1 == "label ")
                            {
                                string valorNodo = "";
                                string[] cadena = rel.Split('#');
                                string[] cadena2 = valor.Split('-');
                                string[] cadena3 = cadena2[0].Split('=');
                                if (cadena2.Length > 1)
                                {
                                    valorNodo = cadena3[1] + ":" + cadena2[1];
                                }
                                else
                                {
                                    valorNodo = cadena3[1];
                                }
                                if ((valorNodo.StartsWith(" Interaction context :")) || (valorNodo.StartsWith(" Protocol")) || (valorNodo.StartsWith(" Pathogen context")) || (valorNodo.StartsWith(" Allele")) || (valorNodo.StartsWith(" Gene -")))//falta pathogencontext la rama mas larga 
                                {
                                    tipo2 = true;
                                }
                                if ((valorNodo.StartsWith(" Description :")))
                                {
                                    tipo3 = true;
                                }
                                string key = "{\"key\":\"";
                                string parent = "\" , \"parent\": \"";
                                string color = "\" ,\"color\":\"";
                     
                                string resultado = key + valorNodo + parent + padre + color + levelColor +"\"" ;
                                json = json + resultado;
                                padre = valorNodo;
                            }
                        }
                    }
                    if (!tipo2 && !tipo3)
                    {
                        int i = 0;
                        int tot = n.Results.Count;
                        // ahora se calcula el has_value si es que existe.
                        foreach (var linea in n.Results)
                        {
                            int counter = 0;
                            string disn = "";
                            string label = "";
                            string rel = "";
                            string valor = "";
                            string[] res = linea.ToString().Split(',');
                            foreach (var linea2 in res)
                            {
                                if (!linea2.StartsWith("@")) {
                                    if (counter == 0)
                                    {
                                        disn = linea2.ToString();
                                    }
                                    if (counter == 1)
                                    {
                                        label = linea2.ToString();
                                    }
                                    if (counter == 2)
                                    {
                                        rel = linea2.ToString();
                                    }
                                    if (counter == 3)
                                    {
                                        valor = linea2.ToString();
                                    }
                                    counter++;
                                    }
                            }
                            string[] prueba = rel.Split('#');
                            if (prueba.Length > 1)
                            {
                                var prueba1 = prueba[1].ToString();
                                if (prueba1 == "has_value ")
                                {
                                    string[] cadena = valor.Split('=');
                                    string texto = ",\"texto\": \"";
                                    string resultado = texto + cadena[1]+ "\"";
                                    json = json + resultado.Replace('$', '"');
                                }
                                i++;
                                if (i == tot)
                                {
                                    string fin = "},";
                                    json = json + fin;
                                }
                            }
                            else
                            {
                                string texto = "\" ,\"texto\": \"\"";
                                string fin = "\"},";
                                json = json + texto + fin;
                            }
                        }
                        // ahora se recorren las relaciones de este nodo
                        foreach (var linea in n.Results)
                        {
                            int counter = 0;
                            string disn = "";
                            string label = "";
                            string rel = "";
                            string valor = "";
                            string[] res = linea.ToString().Split(',');
                            foreach (var linea2 in res)
                            {
                                if (!linea2.StartsWith("@")) {
                                    if (counter == 0)
                                    {
                                        disn = linea2.ToString();
                                    }
                                    if (counter == 1)
                                    {
                                        label = linea2.ToString();
                                    }
                                    if (counter == 2)
                                    {
                                        rel = linea2.ToString();
                                    }
                                    if (counter == 3)
                                    {
                                        valor = linea2.ToString();
                                    }
                                    counter++;
                                }
                            }
                            string[] prueba = rel.Split('#');
                            if (prueba.Length > 1)
                            {
                                var prueba1 = prueba[1].ToString();
                                if ((prueba1 == "has_participant ") || (prueba1 == "has_unique_identifier ") || (prueba1 == "is_manifested_as ") || (prueba1 == "has_unique_identifier "))//relaciones
                                {
                                    //Decclaramos todos los string necesarios para las consultas
                                    string[] k = valor.Split('=');
                                    string url1 = k[1];
                                    string url = url1.TrimStart();
                                    //Cargamos el prefijo para usarlo en todas las consultas
                                    string prefijos = "PREFIX RO: <http://www.obofoundry.org/ro/ro.owl#>PREFIX SIO: <http://semanticscience.org/resource/>PREFIX EDAM:  <http://edamontology.org/>PREFIX PHIO: <http://linkeddata.systems/ontologies/SemanticPHIBase#>PREFIX PUBMED:  <http://linkedlifedata.com/resource/pubmed/>PREFIX rdfs: <http://www.w3.org/2000/01/rdf-schema#>PREFIX up:  <http://purl.uniprot.org/core/>PREFIX foaf: <http://xmlns.com/foaf/0.1/>PREFIX skos: <http://www.w3.org/2004/02/skos/core#>";
                                    string inicio = "SELECT DISTINCT ?disn_1 ?label ?rel ?valor WHERE { ?disn_1 ?rel ?valor . ?disn_1 rdfs:label ?label FILTER(( ?disn_1 = <";
                                    string fin = ">))}";

                                    //Se establece el endpoint para las consultas
                                    SparqlRemoteEndpoint endpoint = new SparqlRemoteEndpoint(new Uri("http://linkeddata.systems:8890/sparql"));

                                    string query = prefijos + inicio + url + fin;
                                    SparqlResultSet resultQuery2 = endpoint.QueryWithResultSet(query);
                                    //recorremos los resultados de esta primera query
                                    if (resultQuery2.Results != null)
                                    {
                                        metodoRecursivo(resultQuery2, padre, level);
                                    }
                                }
                            }
                        }
                    }
                    else if(tipo2)
                    {
                        int i = 0;
                        int tot = n.Results.Count;
                        Boolean finHasValue = false;
                        // ahora se calcula el has_value si es que existe.
                        foreach (var linea in n.Results)
                        {
                            int counter = 0;
                            string disn = "";
                            string label = "";
                            string rel = "";
                            string valor = "";
                            string[] res = linea.ToString().Split(',');
                            
                            foreach (var linea2 in res)
                            {
                                if (!linea2.StartsWith("@"))
                                {
                                    if (counter == 0)
                                    {
                                        disn = linea2.ToString();
                                    }
                                    if (counter == 1)
                                    {
                                        label = linea2.ToString();
                                    }
                                    if (counter == 2)
                                    {
                                        rel = linea2.ToString();
                                    }
                                    if (counter == 3)
                                    {
                                        valor = linea2.ToString();
                                    }
                                    counter++;
                                }
                            }
                            string[] prueba = rel.Split('#');
                            if (prueba.Length > 1)
                            {
                                var prueba1 = prueba[1].ToString();
                                if (prueba1 == "has_value ")
                                {
                                    string[] cadena = valor.Split('=');
                                    string texto = ",\"texto\": \"";
                                    string resultado = texto + cadena[1] + "\"";
                                    json = json + resultado.Replace('$', '"');
                                }
                                i++;
                                if (i == tot)
                                {
                                    string fin = "},";
                                    json = json + fin;
                                }
                            }
                            else
                            {
                                if (!finHasValue)
                                {
                                    string fin = "},";
                                    json = json + fin;
                                    finHasValue = true;
                                }
                            }
                        }
                        // ahora se recorren las relaciones de este nodo
                        foreach (var linea in n.Results)
                        {
                            int counter = 0;
                            string disn = "";
                            string label = "";
                            string rel = "";
                            string valor = "";
                            string[] res = linea.ToString().Split(',');
                            foreach (var linea2 in res)
                            {
                                if (!linea2.StartsWith("@"))
                                {
                                    if (counter == 0)
                                    {
                                        disn = linea2.ToString();
                                    }
                                    if (counter == 1)
                                    {
                                        label = linea2.ToString();
                                    }
                                    if (counter == 2)
                                    {
                                        rel = linea2.ToString();
                                    }
                                    if (counter == 3)
                                    {
                                        valor = linea2.ToString();
                                    }
                                    counter++;
                                }
                            }
                            string[] prueba = rel.Split('#');
                            if (prueba.Length > 1)
                            {
                                var prueba1 = prueba[1].ToString();
                                if ((prueba1 == "depends_on ") || (prueba1 == "has_quality ") || (prueba1 == "is_output_of ") || (prueba1 == "is_variant_of ") || (prueba1 == "has_unique_identifier "))//relaciones
                                {
                                    //Decclaramos todos los string necesarios para las consultas
                                    string[] k = valor.Split('=');
                                    string url1 = k[1];
                                    string url = url1.TrimStart();
                                    //Cargamos el prefijo para usarlo en todas las consultas
                                    string prefijos = "PREFIX RO: <http://www.obofoundry.org/ro/ro.owl#>PREFIX SIO: <http://semanticscience.org/resource/>PREFIX EDAM:  <http://edamontology.org/>PREFIX PHIO: <http://linkeddata.systems/ontologies/SemanticPHIBase#>PREFIX PUBMED:  <http://linkedlifedata.com/resource/pubmed/>PREFIX rdfs: <http://www.w3.org/2000/01/rdf-schema#>PREFIX up:  <http://purl.uniprot.org/core/>PREFIX foaf: <http://xmlns.com/foaf/0.1/>PREFIX skos: <http://www.w3.org/2004/02/skos/core#>";
                                    string inicio = "SELECT DISTINCT ?disn_1 ?label ?rel ?valor WHERE { ?disn_1 ?rel ?valor . ?disn_1 rdfs:label ?label FILTER(( ?disn_1 = <";
                                    string fin = ">))}";

                                    //Se establece el endpoint para las consultas
                                    SparqlRemoteEndpoint endpoint = new SparqlRemoteEndpoint(new Uri("http://linkeddata.systems:8890/sparql"));

                                    string query = prefijos + inicio + url + fin;
                                    SparqlResultSet resultQuery2 = endpoint.QueryWithResultSet(query);
                                    //recorremos los resultados de esta primera query
                                    if (resultQuery2.Results != null)
                                    {
                                        metodoRecursivo(resultQuery2, padre, level);
                                    }
                                }
                            }
                        }
                    }
                    else
                    {
                        int i = 0;
                        int tot = n.Results.Count;
                        // ahora se calcula el (has_value y is_describe_by) si es que existe.
                        foreach (var linea in n.Results)
                        {
                            int counter = 0;
                            string disn = "";
                            string label = "";
                            string rel = "";
                            string valor = "";
                            string[] res = linea.ToString().Split(',');
                            foreach (var linea2 in res)
                            {
                                if (!linea2.StartsWith("@")) {
                                    if (counter == 0)
                                    {
                                        disn = linea2.ToString();
                                    }
                                    if (counter == 1)
                                    {
                                        label = linea2.ToString();
                                    }
                                    if (counter == 2)
                                    {
                                        rel = linea2.ToString();
                                    }
                                    if (counter == 3)
                                    {
                                        valor = linea2.ToString();
                                    }
                                    counter++;
                                }
                            }
                            string[] prueba = rel.Split('#');
                            if (prueba.Length > 1)
                            {
                                var prueba1 = prueba[1].ToString();
                                if (prueba1 == "has_value ")
                                {
                                    string[] cadena = valor.Split('=');
                                    string texto = ", \"texto\": \""; ;
                                    string resultado = texto + cadena[1];
                                    json = json + resultado.Replace('$', '"');
                                }
                                if (prueba1 == "is_described_by ")
                                {
                                    string[] cadena = valor.Split('=');
                                    string resultado = "|url: " + cadena[1];
                                    json = json + resultado;
                                }
                                i++;
                                if (i == tot)
                                {
                                    string fin = "\"},";
                                    json = json + fin;
                                }
                            }
                        }



                        // ahora se recorren las relaciones de este nodo
                        foreach (var linea in n.Results)
                        {
                            int counter = 0;
                            string disn = "";
                            string label = "";
                            string rel = "";
                            string valor = "";
                            string[] res = linea.ToString().Split(',');
                            foreach (var linea2 in res)
                            {
                                if (!linea2.StartsWith("@"))
                                {
                                    if (counter == 0)
                                    {
                                        disn = linea2.ToString();
                                    }
                                    if (counter == 1)
                                    {
                                        label = linea2.ToString();
                                    }
                                    if (counter == 2)
                                    {
                                        rel = linea2.ToString();
                                    }
                                    if (counter == 3)
                                    {
                                        valor = linea2.ToString();
                                    }
                                    counter++;
                                }
                            }
                            string[] prueba = rel.Split('#');
                            if (prueba.Length > 1)
                            {
                                var prueba1 = prueba[1].ToString();
                                if ((prueba1 == "depends_on ") || (prueba1 == "has_quality ") || (prueba1 == "is_output "))//relaciones
                                {
                                    //Decclaramos todos los string necesarios para las consultas
                                    string[] k = valor.Split('=');
                                    string url1 = k[1];
                                    string url = url1.TrimStart();
                                    //Cargamos el prefijo para usarlo en todas las consultas
                                    string prefijos = "PREFIX RO: <http://www.obofoundry.org/ro/ro.owl#>PREFIX SIO: <http://semanticscience.org/resource/>PREFIX EDAM:  <http://edamontology.org/>PREFIX PHIO: <http://linkeddata.systems/ontologies/SemanticPHIBase#>PREFIX PUBMED:  <http://linkedlifedata.com/resource/pubmed/>PREFIX rdfs: <http://www.w3.org/2000/01/rdf-schema#>PREFIX up:  <http://purl.uniprot.org/core/>PREFIX foaf: <http://xmlns.com/foaf/0.1/>PREFIX skos: <http://www.w3.org/2004/02/skos/core#>";
                                    string inicio = "SELECT DISTINCT ?disn_1 ?label ?rel ?valor WHERE { ?disn_1 ?rel ?valor . ?disn_1 rdfs:label ?label FILTER(( ?disn_1 = <";
                                    string fin = ">))}";

                                    //Se establece el endpoint para las consultas
                                    SparqlRemoteEndpoint endpoint = new SparqlRemoteEndpoint(new Uri("http://linkeddata.systems:8890/sparql"));

                                    string query = prefijos + inicio + url + fin;
                                    SparqlResultSet resultQuery2 = endpoint.QueryWithResultSet(query);
                                    //recorremos los resultados de esta primera query
                                    if (resultQuery2.Results != null)
                                    {
                                        metodoRecursivo(resultQuery2, padre, level);
                                    }
                                }
                            }
                        }
                    }
                }
            }
            catch (NotImplementedException queryEx)
            {
                //There was an error executing the query so handle it here
                Console.WriteLine(queryEx.Message);
            }
            return n;
        }
    }
}

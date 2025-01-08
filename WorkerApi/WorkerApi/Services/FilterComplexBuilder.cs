using System.Text;
using WorkerApi.Models.Filters;
using WorkerApi.Models.Graph;

namespace WorkerApi.Services
{
    public class FilterComplexBuilder : IFilterComplexBuilder
    {
        private readonly ILogger<IFilterComplexBuilder> _logger;
        private readonly StringBuilder _inputSection;
        private readonly StringBuilder _filterSection;
        private readonly StringBuilder _outputSection;
        private readonly IDictionary<string, string> _streamLabels;
        private int _inputCounter;
        private bool _hasFilters;

        public FilterComplexBuilder(ILogger<IFilterComplexBuilder> logger)
        {
            _logger = logger;
            _inputSection = new StringBuilder();
            _filterSection = new StringBuilder();
            _outputSection = new StringBuilder();
            _streamLabels = new Dictionary<string, string>();
            _inputCounter = 0;
            _hasFilters = false;
        }

        public void AddInput(FilterVertex inputNode)
        {
            try
            {
                var inputParams = inputNode.GetFilterParams();
                var inputUrl = "";
                if (inputParams.FirstOrDefault() is KeyValuePair<string, string> kvp && kvp.Key == "src")
                {
                    inputUrl = kvp.Value;
                }

                _inputSection.Append($" -i {inputUrl}");

                // Label d'entrée pour le flux vidéo
                var inputLabel = $"[{_inputCounter}:v]";

                // Association du label d'entrée avec tous les flux de sortie de ce noeud
                foreach (var outStream in inputNode.OutStreams)
                {
                    _streamLabels[outStream] = inputLabel;
                }

                _inputCounter++;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error adding input to filter complex");
                throw;
            }
        }

        public void AddFilter(AbstractFilterComplexVertex filter)
        {
            try
            {
                if (!_hasFilters)
                {
                    _filterSection.Append(" -filter_complex \"");
                    _hasFilters = true;
                }

                // Récupération du label d'entrée depuis le dictionnaire
                var inStreamLabel = _streamLabels[filter.InStreams[0]];

                // Construction de la commande du filtre 
                var filterCommand = filter.ComputeFilterComplexOutput();

                // Remplacement du label d'entrée par le vrai label ffmpeg
                filterCommand = filterCommand.Replace($"[{filter.InStreams[0]}]", inStreamLabel);

                // Ajout de la commande à la section des filtres
                _filterSection.Append(filterCommand);

                // Enregistrement des labels de sortie en utilisant les noms exacts du JSON
                foreach (var outStream in filter.OutStreams)
                {
                    _streamLabels[outStream] = $"[{outStream}]";
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error adding filter to filter complex");
                throw;
            }
        }

        public void AddOutput(FilterVertex outputNode)
        {
            try
            {
                var inputStream = outputNode.InStreams[0];

                // Utilisation du label exact du flux d'entrée
                var inputLabel = _streamLabels[inputStream];

                _outputSection.Append($" -map \"{inputLabel}\"");

                // Ajout des paramètres de sortie si présents
                var outputParams = outputNode.GetFilterParams();
                if (outputParams.Length > 0 && outputParams[0] is KeyValuePair<string, string> kvp)
                {
                    _outputSection.Append($" {kvp.Value}");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error adding output to filter complex");
                throw;
            }
        }

        public string BuildFilterComplex()
        {
            try
            {
                var command = new StringBuilder();

                // Ajout des entrées
                command.Append(_inputSection);

                // Ajout des filtres
                if (_hasFilters)
                {
                    command.Append(_filterSection);
                    command.Append("\"");
                }

                // Ajout des sorties
                command.Append(_outputSection);

                return command.ToString();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error building filter complex");
                throw;
            }
        }
    }
}
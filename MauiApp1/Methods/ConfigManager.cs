using System.Text.Json;
using Anti_Bunda_Mole.Models;

namespace Anti_Bunda_Mole.Methods;

public class ConfigManager
{
    private static ConfigManager? _instance;
    public Configuracoes Config { get; private set; }

    private string FilePath { get; }

    private ConfigManager()
    {
        var folder = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
        FilePath = Path.Combine(folder, "AntiBundaMole", "config.json");

        Config = new Configuracoes
        {
            Dias = new Dictionary<int, DiaConfig>(),
            IntervaloAviso = 0,
            PosicaoTarefas = ""
        };
    }

    public static ConfigManager Instance => _instance ??= new ConfigManager();

    public async Task LoadAsync()
    {
        if (!File.Exists(FilePath)) return;

        try
        {
            var json = await File.ReadAllTextAsync(FilePath);

            // Compatibilidade: se JSON antigo tiver string, tenta converter para int
            var tempConfig = JsonSerializer.Deserialize<JsonElement>(json);

            if (tempConfig.TryGetProperty("Dias", out var diasElement))
            {
                var diasInt = new Dictionary<int, DiaConfig>();
                foreach (var prop in diasElement.EnumerateObject())
                {
                    if (int.TryParse(prop.Name, out int diaNumero))
                    {
                        var diaConfig = JsonSerializer.Deserialize<DiaConfig>(prop.Value.GetRawText());
                        if (diaConfig != null)
                            diasInt[diaNumero] = diaConfig;
                    }
                }

                var intervalo = tempConfig.TryGetProperty("IntervaloAviso", out var intervalEl) ? intervalEl.GetInt32() : 0;
                var posicao = tempConfig.TryGetProperty("PosicaoTarefas", out var posEl) ? posEl.GetString() ?? "" : "";

                Config = new Configuracoes
                {
                    Dias = diasInt,
                    IntervaloAviso = intervalo,
                    PosicaoTarefas = posicao
                };
            }
        }
        catch
        {
            // Pode logar o erro aqui
        }
    }

    public async Task SaveAsync()
    {
        Directory.CreateDirectory(Path.GetDirectoryName(FilePath)!);
        var json = JsonSerializer.Serialize(Config, new JsonSerializerOptions { WriteIndented = true });
        await File.WriteAllTextAsync(FilePath, json);
        ConfigChanged?.Invoke();
    }

    public void LoadIfNeeded()
    {
        if (!File.Exists(FilePath)) return;

        try
        {
            var json = File.ReadAllText(FilePath);
            var config = JsonSerializer.Deserialize<Configuracoes>(json);
            if (config != null) Config = config;
        }
        catch
        {
            // Pode logar o erro aqui
        }
    }

    public event Action? ConfigChanged;

    public void NotifyChanged()
    {
        ConfigChanged?.Invoke();
    }
}

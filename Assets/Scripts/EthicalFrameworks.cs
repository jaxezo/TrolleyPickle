using UnityEngine;

public static class EthicalFrameworks
{
    public enum Frameworks
    {
        Deontology,
        Utilitarianism,
        Care
    }

    public static string FrmaeworkToString(Frameworks framework)
    {
        switch (framework)
        {
            case Frameworks.Deontology:
                return "Deontology";
            case Frameworks.Utilitarianism:
                return "Utilitarianism";
            case Frameworks.Care:
                return "Feminist Ethics of Care";
            default:
                return "ERR";
        }
    }
}

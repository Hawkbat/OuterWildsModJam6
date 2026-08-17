using System.Text;
using System.Xml;

namespace GhostInTheMachine.Controllers;

// Swaps a NomaiText between the text blocks it was built with and a replacement set of translation keys using the same translation method as New Horizons
public class NomaiTextSwapper(NomaiText text, string[] replacementKeys)
{
    readonly NomaiText text = text;
    readonly string[] replacementKeys = replacementKeys;
    readonly XmlNode originalRoot = ParseNomaiObject(OWUtilities.RemoveByteOrderMark(text._nomaiTextAsset));
    readonly XmlNode replacementRoot = ParseNomaiObject(BuildNomaiObject(replacementKeys));

    public void RegisterTranslations()
    {
        var table = TextTranslation.Get().m_table.theTable;
        foreach (var key in replacementKeys)
        {
            table[key] = GhostInTheMachine.NewHorizons.GetTranslationForDialogue(key);
        }
    }

    public void SetReplaced(bool replaced)
    {
        text.SetNewXmlData(replaced ? replacementRoot : originalRoot);
    }

    static string BuildNomaiObject(string[] keys)
    {
        var builder = new StringBuilder("<NomaiObject>");
        for (var i = 0; i < keys.Length; i++)
        {
            builder.Append($"<TextBlock><ID>{i + 1}</ID><Text>{keys[i]}</Text></TextBlock>");
        }
        builder.Append("</NomaiObject>");
        return builder.ToString();
    }

    static XmlNode ParseNomaiObject(string xml)
    {
        var document = new XmlDocument();
        document.LoadXml(xml);
        return document.SelectSingleNode("NomaiObject");
    }
}

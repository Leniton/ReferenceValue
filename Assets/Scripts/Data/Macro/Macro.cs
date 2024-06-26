
public class Macro : IMacro
{
    Keyword macro;
    private IValueReference<int> bonusData;
    private RollData rollData;

    public Macro(IValueReference<int> _bonusData, Keyword dieFormula)
    {
        bonusData = _bonusData;
        macro = dieFormula;
        int id = dieFormula.description.ToLower().IndexOf('d');
        rollData = id >= 0 ? new RollData(GetDieNumber(dieFormula.description, id), GetNumberOfDies(dieFormula.description, id)) : null;
    }

    private int GetDieNumber(string value, int dID)
    {
        string numbers = string.Empty;
        for (int i = dID + 1; i < value.Length; i++)
        {
            if (StringUtil.IsNumber(value[i]))
            {
                numbers += value[i];
            }
            else break;
        }
        if (string.IsNullOrEmpty(numbers)) return -1;

        return int.Parse(numbers);
    }
    private int GetNumberOfDies(string value, int dID)
    {
        string numbers = string.Empty;
        for (int i = dID - 1; i > 0; i--)
        {
            if (StringUtil.IsNumber(value[i]))
            {
                numbers = numbers.Insert(0, $"{value[i]}");
            }
            else break;
        }
        if (string.IsNullOrEmpty(numbers)) return 1;

        return int.Parse(numbers);
    }

    public string UseMacro()
    {
        string result = $"{macro} ({macro.description}) | ";

        if (rollData == null)
        {
            result += bonusData.GetValue().ToString();
        }
        else
        {
            result += Roll(rollData.type, rollData.amount, bonusData.GetValue());
        }

        return result;
    }

    public string Roll(int diceType = 20, int diceAmount = 1, int bonus = 0)
    {
        int value;
        DiceRoll[] dices = new DiceRoll[diceAmount];
        ITaggedOperation operation = new SumOperation();
        IValueReference<int> diceRoll = new NumberReference(bonus);

        for (int i = 0; i < diceAmount; i++)
        {
            dices[i] = new DiceRoll(diceType);
            diceRoll = new ValueMerger(diceRoll, dices[i], operation);
        }
        value = diceRoll.GetValue();

        string debug = "";
        string bonusString = bonus != 0 ? $" {operation.key} {bonus}" : "";
        for (int i = 0; i < dices.Length; i++)
        {
            string extra = i < diceAmount - 1 ? $" {operation.key}" : "";
            debug += $"{dices[i].lastRoll}{extra}";
        }
        debug += bonusString;

        return $"{diceAmount}d{diceType}{bonusString} = {value} [{debug}]";
    }
}

public class RollData
{
    public int type, amount;
    public RollData(int type, int amount)
    {
        this.type = type;
        this.amount = amount;
    }
}
namespace FateCalculator.Models
{
    public class Skill
    {
        public string Name { get; set; } = "";
        public string Description { get; set; } = "";
        public int ManaCost { get; set; } = 0; // 0 = не тратит ману
        public bool IsNoblePhantasm { get; set; } = false;
    }
}

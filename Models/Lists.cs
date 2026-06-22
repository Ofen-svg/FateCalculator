using System.Collections.Generic;

namespace FateCalculator.Models
{
    public static class Lists
    {
        public static readonly string[] Classes = new[]
        {
            "Сейбер", "Лансер", "Арчер", "Ассассин",
            "Райдер", "Кастер", "Берсеркер", "Экстра", "Мастер"
        };

        public static readonly string[] Alignments = new[]
        {
            "Законопослушное・Доброе",
            "Законопослушное・Нейтральное",
            "Законопослушное・Злое",
            "Нейтральное・Доброе",
            "Истинный нейтрал",
            "Нейтральное・Злое",
            "Хаотическое・Доброе",
            "Хаотическое・Нейтральное",
            "Хаотическое・Злое"
        };

        // Полный список рангов, по возрастанию силы (важно для очереди ходов).
        public static readonly string[] RankOrder = new[]
        {
            "E","E+","E++",
            "D","D+","D++",
            "C","C+","C++",
            "B","B+","B++",
            "A","A+","A++",
            "EX","EX+","EX++"
        };

        public static int RankIndex(string rank)
        {
            var i = System.Array.IndexOf(RankOrder, rank);
            return i < 0 ? 0 : i;
        }

        // Классы-слуги (на них действует регенерация 5HP/3 Маны в минуту).
        public static readonly HashSet<string> ServantClasses = new HashSet<string>
        {
            "Сейбер","Лансер","Арчер","Ассассин","Райдер","Кастер","Берсеркер","Экстра"
        };
    }
}

namespace Server.Data
{
    [Serializable]
    public class ProjectileStat
    {
        public int ID;
        public string Name;
        public int Speed;
        public int Range;
        public string PrefabPath;
    }

    [Serializable]
    public class ProjectileStatData : ILoader<int, ProjectileStat>
    {
        #region Variables

        public List<ProjectileStat> ProjectileStatList = new();

        #endregion Variables

        #region Methods

        public Dictionary<int, ProjectileStat> MakeDictionary()
        {
            Dictionary<int, ProjectileStat> dictionary = new();

            foreach (ProjectileStat stat in ProjectileStatList)
            {
                dictionary.Add(stat.ID, stat);
            }

            return dictionary;
        }

        #endregion Methods
    }
}
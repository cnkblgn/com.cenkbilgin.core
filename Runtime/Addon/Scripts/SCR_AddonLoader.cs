using System.Threading.Tasks;

namespace Core.Addon
{
    public interface IAddonLoader
    {
        public Task LoadAsync();
        public void Build();
    }
}

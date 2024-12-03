using Helix.Genetics;
using Helix.Genetics.Reproduction.Asexual;
using Helix.Genetics.Reproduction.Sexual;
using Ninject.Modules;

namespace Helix.Ninject
{
    public class HelixModule : NinjectModule
    {
        public override void Load()
        {
            Bind<EvolutionRunner>().ToSelf();

            Bind<AsexualReproduction>().ToSelf();
            Bind<SexualReproduction>().ToSelf();
            
        }
    }
}
namespace WEDT.Searcher
{
    using Nancy;
    using Nancy.TinyIoc;
    using System.IO;
    using WEDT.PolishLangAnalyzer;

    public class Bootstrapper : DefaultNancyBootstrapper
    {
        protected override void ConfigureApplicationContainer(TinyIoCContainer container)
        {
            base.ConfigureApplicationContainer(container);

            var dictPath = Path.Combine(RootPathProvider.GetRootPath(), "Content", "polimorf-20141120.tab");
            container.Register<IPolishStemmer>(new PolishStemmer(dictPath));
        }
    }
}
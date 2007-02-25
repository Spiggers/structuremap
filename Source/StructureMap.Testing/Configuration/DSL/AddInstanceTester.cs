using System;
using System.Configuration;
using NUnit.Framework;
using StructureMap.Configuration.DSL;
using StructureMap.Graph;
using StructureMap.Testing.Widget;
using IList=System.Collections.IList;

namespace StructureMap.Testing.Configuration.DSL
{
    [TestFixture]
    public class AddInstanceTester
    {
        private InstanceManager manager;
        private PluginGraph pluginGraph;

        [SetUp]
        public void SetUp()
        {
            pluginGraph = new PluginGraph();
            Registry registry = new Registry(pluginGraph);

            // Add an instance with properties
            registry.AddInstanceOf<IWidget>()
                .WithName("DarkGreen")
                .UsingConcreteType<ColorWidget>()
                .WithProperty("Color").EqualTo("DarkGreen");

            // Add an instance by specifying the ConcreteKey
            registry.AddInstanceOf<IWidget>()
                .WithName("Purple")
                .UsingConcreteTypeNamed("Color")
                .WithProperty("Color").EqualTo("Purple");

            // Pull a property from the App config
            registry.AddInstanceOf<IWidget>()
                .WithName("AppSetting")
                .UsingConcreteType<ColorWidget>()
                .WithProperty("Color").EqualToAppSetting("Color");




            registry.AddInstanceOf<IWidget>().UsingConcreteType<AWidget>();


            


            /*
            


                                                // Build an instance for IWidget, then setup StructureMap to return cloned instances of the 
                                                // "Prototype" (GoF pattern) whenever someone asks for IWidget named "Jeremy"
                                                registry.AddInstanceOf<IWidget>().WithName("Jeremy").UsePrototype(new CloneableWidget("Jeremy"));
            
                                                // Return the specific instance when an IWidget named "Julia" is requested
                                                registry.AddInstanceOf<IWidget>( new CloneableWidget("Julia") ).WithName("Julia");
                                                    */
            manager = registry.BuildInstanceManager();
        }




        [Test]
        public void SpecifyANewInstanceWithADependency()
        {
            Registry registry = new Registry();

            // Specify a new Instance, create an instance for a dependency on the fly
            string instanceKey = "OrangeWidgetRule";
            registry.AddInstanceOf<Rule>().UsingConcreteType<WidgetRule>().WithName(instanceKey)
                .Child<IWidget>().Is(
                    Registry.Instance<IWidget>().UsingConcreteType<ColorWidget>()
                        .WithProperty("Color").EqualTo("Orange")
                        .WithName("Orange")
                        );

            InstanceManager mgr = registry.BuildInstanceManager();

            ColorWidget orange = (ColorWidget) mgr.CreateInstance<IWidget>("Orange");
            Assert.IsNotNull(orange);

            WidgetRule rule = (WidgetRule)mgr.CreateInstance<Rule>(instanceKey);
            ColorWidget widget = (ColorWidget) rule.Widget;
            Assert.AreEqual("Orange", widget.Color);
        }


        [Test]
        public void AddInstanceAndOverrideTheConcreteTypeForADependency()
        {
            Registry registry = new Registry();

            // Specify a new Instance that specifies the concrete type used for a dependency
            registry.AddInstanceOf<Rule>().UsingConcreteType<WidgetRule>().WithName("AWidgetRule")
                .Child<IWidget>().IsConcreteType<AWidget>();

            manager = registry.BuildInstanceManager();

            WidgetRule rule = (WidgetRule)manager.CreateInstance<Rule>("AWidgetRule");
            Assert.IsInstanceOfType(typeof(AWidget), rule.Widget);            
        }

        [Test]
        public void AddAnInstanceWithANameAndAPropertySpecifyingConcreteType()
        {
            ColorWidget widget = (ColorWidget) manager.CreateInstance<IWidget>("DarkGreen");
            Assert.AreEqual("DarkGreen", widget.Color);
        }

        [Test]
        public void AddAnInstanceWithANameAndAPropertySpecifyingConcreteKey()
        {
            ColorWidget widget = (ColorWidget) manager.CreateInstance<IWidget>("Purple");
            Assert.AreEqual("Purple", widget.Color);
        }

        [Test]
        public void CreateAnInstancePullAPropertyFromTheApplicationConfig()
        {
            Assert.AreEqual("Blue", ConfigurationManager.AppSettings["Color"]);
            ColorWidget widget = (ColorWidget) manager.CreateInstance<IWidget>("AppSetting");
            Assert.AreEqual("Blue", widget.Color);
        }

        [Test]
        public void SimpleCaseWithNamedInstance()
        {
            Registry registry = new Registry();

            // Specify a new Instance and override the Name
            registry.AddInstanceOf<IWidget>().UsingConcreteType<AWidget>().WithName("MyInstance");

            manager = registry.BuildInstanceManager();

            AWidget widget = (AWidget) manager.CreateInstance<IWidget>("MyInstance");
            Assert.IsNotNull(widget);
        }

        [Test]
        public void SimpleCaseByPluginName()
        {
            AWidget widget = (AWidget) manager.CreateInstance<IWidget>("AWidget");
            Assert.IsNotNull(widget);
        }

        [Test]
        public void SpecifyANewInstanceOverrideADependencyWithANamedInstance()
        {
            Registry registry = new Registry();

            registry.ScanAssemblies().IncludeAssemblyContainingType<IWidget>();

            registry.AddInstanceOf<Rule>().UsingConcreteType<ARule>().WithName("Alias");

            // Add an instance by specifying the ConcreteKey
            registry.AddInstanceOf<IWidget>()
                .WithName("Purple")
                .UsingConcreteTypeNamed("Color")
                .WithProperty("Color").EqualTo("Purple");

            // Specify a new Instance, override a dependency with a named instance
            registry.AddInstanceOf<Rule>().UsingConcreteType<WidgetRule>().WithName("RuleThatUsesMyInstance")
                .Child("widget").IsNamedInstance("Purple");

            manager = registry.BuildInstanceManager();

            Assert.IsInstanceOfType(typeof(ARule), manager.CreateInstance<Rule>("Alias"));

            WidgetRule rule = (WidgetRule) manager.CreateInstance<Rule>("RuleThatUsesMyInstance");
            ColorWidget widget = (ColorWidget) rule.Widget;
            Assert.AreEqual("Purple", widget.Color);
        }
    }


    public class WidgetRule : Rule
    {
        private readonly IWidget _widget;

        public WidgetRule(IWidget widget)
        {
            _widget = widget;
        }


        public IWidget Widget
        {
            get { return _widget; }
        }
    }

    public class WidgetThing : IWidget
    {
        public void DoSomething()
        {
            throw new NotImplementedException();
        }
    }

    public class CloneableWidget : IWidget, ICloneable
    {
        private string _name;


        public CloneableWidget(string name)
        {
            _name = name;
        }

        public string Name
        {
            get { return _name; }
        }

        public void DoSomething()
        {
            throw new NotImplementedException();
        }

        public object Clone()
        {
            return MemberwiseClone();
        }
    }

    public class ARule : Rule
    {
        
    }
}
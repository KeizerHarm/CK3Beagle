using CK3BeagleServer.Core.Domain;
using CK3BeagleServer.Core.Domain.Entities;
using CK3BeagleServer.Core.Generated;

namespace CK3BeagleServer.Core.Parsing
{
    public class SecondPassTests : BaseParserTest
    {
        [Fact]
        public void EventFileDecoratedProperly()
        {
            //arrange
            string[] effects = [
                "if", "send_interface_toast", "custom_tooltip", "else"
                ];
            string[] triggers = [
                "gold", "prestige", "not"
                ];
            string[] eventTargets = [
                "root", "scope"
                ];


            //act
            var parsedFile = GetTestCase("TwoEvents", expectedDeclarationType: DeclarationType.Event,
                effects: effects, triggers: triggers, eventTargets: eventTargets);

            //assert
            Assert.Equal(4, parsedFile.Children.Count);
            Assert.Equal(2, parsedFile.Declarations.Count);

            var firstComment = parsedFile.Children[0];
            Assert.True(firstComment is Comment comment
                && comment.RawWithoutHashtag == "Send a toast to the imprisoner if the person they captured is worth war score."
                && comment.NodeType == NodeType.NonStatement);

            var secondComment = parsedFile.Children[2];
            Assert.True(secondComment is Comment comment2
                && comment2.RawWithoutHashtag == "Send a toast to the primary defender if the person that was captured is worth war score (and is not them)."
                && comment2.NodeType == NodeType.NonStatement);

            var firstEvent = parsedFile.Children[1];
            Assert.True(firstEvent is Declaration _);
            Declaration event1 = (Declaration)firstEvent;
            Assert.True(event1.NodeType == NodeType.NonStatement
                && event1.DeclarationType == DeclarationType.Event);

            Assert.Equal(2, event1.Children.Count);
            Assert.True(event1.Children[0] is BinaryExpression binExp1
                && binExp1.Key == "hidden"
                && binExp1.Value == "yes"
                && binExp1.NodeType == NodeType.NonStatement);

            Assert.True(event1.Children[1] is NamedBlock _);
            NamedBlock immediate = (NamedBlock)event1.Children[1];
            Assert.True(immediate.NodeType == NodeType.NonStatement
                && immediate.Children[0] is NamedBlock ifChild
                && ifChild.NodeType == NodeType.Effect
                && ifChild.Children.Count == 2
                && ifChild.Children[0] is NamedBlock _
                && ifChild.Children[1] is NamedBlock _);
            NamedBlock limit = (NamedBlock)((NamedBlock)immediate.Children[0]).Children[0];

            Assert.True(limit.NodeType == NodeType.NonStatement
                && limit.Children.Count == 3
                && limit.Children.All(x => x.NodeType == NodeType.Trigger));

            NamedBlock sendInterfaceToast1 = (NamedBlock)((NamedBlock)immediate.Children[0]).Children[1];


            Assert.True(sendInterfaceToast1.NodeType == NodeType.Effect
                && sendInterfaceToast1.Children[0].NodeType == NodeType.NonStatement
                && sendInterfaceToast1.Children[1].NodeType == NodeType.NonStatement
                && sendInterfaceToast1.Children[2].NodeType == NodeType.NonStatement
                && sendInterfaceToast1.Children[3].NodeType == NodeType.Effect
                );

            Assert.Equal(11, event1.GetSize());
        }

        [Fact]
        public void AccoladeNameDecoratedProperly()
        {
            //arrange
            string[] effects = [];
            string[] triggers = [
                "or", "exists", "nor", "always"
                ];
            string[] eventTargets = [
                "liege", "capital_county", "scope", "title_province"
                ];


            //act
            var parsedFile = GetTestCase("AccoladeName", expectedDeclarationType: DeclarationType.AccoladeName,
                effects: effects, triggers: triggers, eventTargets: eventTargets);

            //assert
            Assert.Equal(2, parsedFile.Children.Count);
            Assert.Single(parsedFile.Declarations);

            var accolade = parsedFile.Declarations.First();
            Assert.Equal(SymbolType.AccoladeName, accolade.SymbolType);

            var potential = accolade.Children.OfType<NamedBlock>().Single(x => x.Key == "potential");
            Assert.Equal(SymbolType.AccoladeName_Potential, potential.SymbolType);

            var options = accolade.Children.OfType<NamedBlock>().Where(x => x.Key == "option").ToList();
            Assert.Equal(2, options.Count);
            Assert.Equal(SymbolType.AccoladeName_Option, options[0].SymbolType);
            Assert.Equal(SymbolType.AccoladeName_Option, options[1].SymbolType);

            var option_trigger = options[0].Children.OfType<NamedBlock>().Single(x => x.Key == "trigger");
            Assert.Equal(SymbolType.AccoladeName_Option_Trigger, option_trigger.SymbolType);

            foreach (var triggerChild in option_trigger.Children)
            {
                Assert.Equal(NodeType.Trigger, triggerChild.NodeType);
            }

            foreach (var triggerChild in option_trigger.Children.OfType<NamedBlock>().SelectMany(x => x.Children))
            {
                Assert.Equal(NodeType.Trigger, triggerChild.NodeType);
            }
        }
    }
}

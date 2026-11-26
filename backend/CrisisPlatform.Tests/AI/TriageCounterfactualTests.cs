using CrisisPlatform.Api.Modules.AI;

namespace CrisisPlatform.Tests.AI;

public class TriageCounterfactualTests
{
    [Fact]
    public void Evaluate_ShouldProduceDeterministicAndAccurateCounterfactualNotes()
    {
        var engine = new TriageEngine();
        var input = new TriageInput("Medical", "Elderly patient trapped in flood water.", 5, 4, "elderly,medical");

        var first = engine.Evaluate(input);
        var second = engine.Evaluate(input);

        Assert.Equal(first.CounterfactualNote, second.CounterfactualNote);
        Assert.Contains("Priority would drop", first.CounterfactualNote);
    }

    [Fact]
    public void Evaluate_LowPriority_ShouldExplainHowPriorityWouldRise()
    {
        var engine = new TriageEngine();
        var low = engine.Evaluate(new TriageInput("Food", "General food inquiry.", 1, 1));

        Assert.Equal("Low", low.Priority);
        Assert.Contains("Priority would rise", low.CounterfactualNote);
    }
}

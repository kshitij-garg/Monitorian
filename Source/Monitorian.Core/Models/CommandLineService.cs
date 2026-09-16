using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace Monitorian.Core.Models;

internal enum CommandLineAction
{
	Get,
	Set
}

internal enum CommandLineMetric
{
	Brightness,
	Contrast
}

internal sealed class CommandLineCommand
{
	public CommandLineAction Action { get; }
	public CommandLineMetric Metric { get; }
	public string Target { get; }
	public int Value { get; }
	public bool IsRelative { get; }

	public CommandLineCommand(
		CommandLineAction action,
		CommandLineMetric metric,
		string target = null,
		int value = 0,
		bool isRelative = false)
	{
		Action = action;
		Metric = metric;
		Target = target;
		Value = value;
		IsRelative = isRelative;
	}
}

internal static class CommandLineService
{
	private const string GetOption = "/get";
	private const string SetOption = "/set";
	private const string ContrastToken = "contrast";

	public static bool ContainsCommand(IReadOnlyCollection<string> arguments) =>
		arguments?.Any(IsCommandOption) is true;

	public static bool TryParse(
		IReadOnlyList<string> arguments,
		out CommandLineCommand command,
		out string error)
	{
		command = null;
		error = null;

		if (arguments is null)
		{
			error = "No command was provided.";
			return false;
		}

		var optionIndex = -1;
		for (var i = 0; i < arguments.Count; i++)
		{
			if (IsCommandOption(arguments[i]))
			{
				optionIndex = i;
				break;
			}
		}

		if (optionIndex < 0)
		{
			error = "No supported command was provided.";
			return false;
		}

		var action = string.Equals(arguments[optionIndex], GetOption, StringComparison.OrdinalIgnoreCase)
			? CommandLineAction.Get
			: CommandLineAction.Set;
		var operands = arguments.Skip(optionIndex + 1).ToList();
		var metric = CommandLineMetric.Brightness;

		if (operands.Count > 0
			&& string.Equals(operands[0], ContrastToken, StringComparison.OrdinalIgnoreCase))
		{
			metric = CommandLineMetric.Contrast;
			operands.RemoveAt(0);
		}

		if (action is CommandLineAction.Get)
		{
			command = new CommandLineCommand(action, metric, JoinTarget(operands));
			return true;
		}

		if (operands.Count == 0)
		{
			error = "A value from 0 to 100, or a signed relative value, is required.";
			return false;
		}

		var valueText = operands[operands.Count - 1];
		if (!int.TryParse(valueText, NumberStyles.Integer, CultureInfo.InvariantCulture, out var value))
		{
			error = $"Invalid value '{valueText}'.";
			return false;
		}

		var isRelative = valueText.StartsWith("+", StringComparison.Ordinal)
			|| valueText.StartsWith("-", StringComparison.Ordinal);
		if (!isRelative && value is < 0 or > 100)
		{
			error = "Absolute values must be between 0 and 100.";
			return false;
		}

		operands.RemoveAt(operands.Count - 1);
		command = new CommandLineCommand(action, metric, JoinTarget(operands), value, isRelative);
		return true;
	}

	private static bool IsCommandOption(string value) =>
		string.Equals(value, GetOption, StringComparison.OrdinalIgnoreCase)
		|| string.Equals(value, SetOption, StringComparison.OrdinalIgnoreCase);

	private static string JoinTarget(IEnumerable<string> values)
	{
		var target = string.Join(" ", values).Trim();
		return string.IsNullOrEmpty(target) ? null : target;
	}
}

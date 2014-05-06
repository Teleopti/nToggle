﻿using System;
using System.Collections.Generic;
using System.Text;
using Toggle.Net.Internal;
using Toggle.Net.Specifications;

namespace Toggle.Net.Providers.TextFile
{
	/// <summary>
	/// Reads features and their <see cref="IToggleSpecification"/> from a text file.
	/// 
	/// Format
	/// [feature]=[specification]
	/// [feature].[specification].[param]=[value]
	/// 
	/// To remark, use "#" sign.
	/// 
	/// <example>
	/// # This is an example
	/// 
	/// NewFeature = false
	/// Logon = true
	/// TheThing = myspecification
	/// TheThing.myspecification.MyParam = 13
	/// TheThing.myspecification.MyOtherParam = 13
	/// </example>
	/// </summary>
	public class FileProviderFactory : IFeatureProviderFactory
	{
		public const string MustContainEqualSign = "Missing equal sign at line {0}.";
		public const string MustOnlyContainOneEqualSign = "More than one equal sign at line {0}.";
		public const string MustHaveValidSpecification = "Unknown specification '{0}' at line {1}.";
		public const string MustHaveTwoDotsIfParameterUse =
			"Wrong parameter usage at line {0}. Use format [feature].[specification].[parametername] = [parametervalue].";
		public const string MustOnlyContainSameParameterOnce = "Parameter '{0}' declared twice at line {1}.";
		public const string MustDeclareSpecificationBeforeParameter = "Declare specification for feature '{0}' before setting parameter at line {1}.";

		private readonly IFileReader _fileReader;
		private readonly ISpecificationMappings _specificationMappings;

		public FileProviderFactory(IFileReader fileReader, ISpecificationMappings specificationMappings)
		{
			_fileReader = fileReader;
			_specificationMappings = specificationMappings;
		}

		public IFeatureProvider Create()
		{
			var exOutput = new StringBuilder();
			var featureSettings = parseFile(_specificationMappings.NameSpecificationMappings(), exOutput);
			foreach (var feature in featureSettings)
			{
				try
				{
					feature.Value.Validate(feature.Key);
				}
				catch (InvalidSpecificationParameterException ex)
				{
					exOutput.AppendLine(ex.Message);
				}
			}
			if (exOutput.Length > 0)
				throw new IncorrectTextFileException(exOutput.ToString());
			return new StaticFeatureProvider(featureSettings);
		}

		private IDictionary<string, Feature> parseFile(IDictionary<string, IToggleSpecification> specificationMappings, StringBuilder exOutput)
		{
			var readFeatures = new Dictionary<string, Feature>(StringComparer.OrdinalIgnoreCase);
			var content = _fileReader.Content();
			for (var index = 0; index < content.Length; index++)
			{
				var row = content[index];
				var rowNumber = index + 1;
				var splitByEqualSign = row.Split('=');
				var leftOfEqualSign = splitByEqualSign[0].Trim();
				if (leftOfEqualSign == string.Empty || leftOfEqualSign.StartsWith("#"))
					continue;
				switch (splitByEqualSign.Length)
				{
					case 1:
						exOutput.AppendLine(string.Format(MustContainEqualSign, rowNumber));
						break;
					case 2:
						var rightOfEqualSign = splitByEqualSign[1].Trim();
						parseRow(readFeatures, specificationMappings, leftOfEqualSign, rightOfEqualSign, rowNumber, exOutput);
						break;
					default:
						exOutput.AppendLine(string.Format(MustOnlyContainOneEqualSign, rowNumber));
						break;
				}
			}
			return readFeatures;
		}

		private static void parseRow(IDictionary<string, Feature> readFeatures,
														IDictionary<string, IToggleSpecification> specificationMappings,
														string leftOfEqualSign, 
														string rightOfEqualSign, 
														int rowNumber, 
														StringBuilder exOutput)
		{
			var splitLeftByDots = leftOfEqualSign.Split('.');
			switch (splitLeftByDots.Length)
			{
				case 1:
					IToggleSpecification foundSpecification;
					if (specificationMappings.TryGetValue(rightOfEqualSign, out foundSpecification))
					{
						if (readFeatures.ContainsKey(leftOfEqualSign))
						{
							readFeatures[leftOfEqualSign].AddSpecification(foundSpecification);
						}
						else
						{
							readFeatures.Add(leftOfEqualSign, new Feature(foundSpecification));
						}
					}
					else
					{
						exOutput.AppendLine(string.Format(MustHaveValidSpecification, rightOfEqualSign, rowNumber));
					}
					break;
				case 3:
					var feature = splitLeftByDots[0];
					var specification = splitLeftByDots[1];
					var paramName = splitLeftByDots[2].Trim();
					var paramValue = rightOfEqualSign;
					try
					{
						Feature featureInstance;
						if(readFeatures.TryGetValue(feature, out featureInstance))
						{
							featureInstance.AddParameter(specificationMappings[specification], paramName, paramValue);
						}
						else
						{
							exOutput.AppendLine(string.Format(MustDeclareSpecificationBeforeParameter, feature, rowNumber));
						}
					}
					catch (ArgumentException)
					{
						exOutput.AppendLine(string.Format(MustOnlyContainSameParameterOnce, paramName, rowNumber));
					}
					break;
				default:
					exOutput.AppendLine(string.Format(MustHaveTwoDotsIfParameterUse, rowNumber));
					break;
			}
		}
	}
}
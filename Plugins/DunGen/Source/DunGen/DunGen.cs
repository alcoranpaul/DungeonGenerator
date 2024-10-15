using System;
using System.Collections.Generic;
using FlaxEngine;
using FlaxEngine.Utilities;


namespace DunGen
{
	/// <summary>
	/// The sample game plugin.
	/// </summary>
	/// <seealso cref="GamePlugin" />
	public class DunGen : GamePlugin
	{
		public Generator Generator { get; private set; }
		/// <inheritdoc />
		public DunGen()
		{
			_description = new PluginDescription
			{
				Name = "DunGen",
				Category = "Other",
				Author = "",
				AuthorUrl = null,
				HomepageUrl = null,
				RepositoryUrl = "https://github.com/FlaxEngine/DunGen",
				Description = "This is an example plugin project.",
				Version = new Version(),
				IsAlpha = false,
				IsBeta = false,
			};
		}

		/// <inheritdoc />
		public override void Initialize()
		{
			base.Initialize();
			Debug.Log("DunGen Initialize");
			if (Generator.Instance == null)
				Generator = new Generator();
			else Generator = Generator.Instance;

		}

		/// <inheritdoc />
		public override void Deinitialize()
		{
			// Use it to cleanup data
			base.Deinitialize();
		}

		public void GenerateDungeon()
		{
			if (Generator != null)
				Generator.GenerateDungeon();
		}
	}




}

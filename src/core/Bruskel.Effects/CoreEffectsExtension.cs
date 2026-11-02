//
// CoreEffectsExtension.cs
//
// Author:
//       Jonathan Pobst <monkey@jpobst.com>
//
// Copyright (c) 2011 Jonathan Pobst
//
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
//
// The above copyright notice and this permission notice shall be included in
// all copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
// THE SOFTWARE.

using System;
using Mono.Addins;
using Bruskel.Core;

[assembly: Addin ("DefaultEffects", BruskelCore.ApplicationVersion, Category = "Core")]
[assembly: AddinName ("Default Effects")]
[assembly: AddinDescription ("The default adjustments and effects that ship with Bruskel")]
[assembly: AddinDependency ("Bruskel", BruskelCore.ApplicationVersion)]
[assembly: AddinFlags (Mono.Addins.Description.AddinFlags.Hidden | Mono.Addins.Description.AddinFlags.CantUninstall)]

namespace Bruskel.Effects;

[Mono.Addins.Extension]
internal sealed class CoreEffectsExtension : IExtension
{
	#region IExtension Members
	public void Initialize ()
	{
		IServiceProvider services = BruskelCore.Services;

		// Add the adjustments
		BruskelCore.Effects.RegisterAdjustment (new AutoLevelEffect (services));
		BruskelCore.Effects.RegisterAdjustment (new BlackAndWhiteEffect (services));
		BruskelCore.Effects.RegisterAdjustment (new BrightnessContrastEffect (services));
		BruskelCore.Effects.RegisterAdjustment (new CurvesEffect (services));
		BruskelCore.Effects.RegisterAdjustment (new HueSaturationEffect (services));
		BruskelCore.Effects.RegisterAdjustment (new InvertColorsEffect (services));
		BruskelCore.Effects.RegisterAdjustment (new LevelsEffect (services));
		BruskelCore.Effects.RegisterAdjustment (new PosterizeEffect (services));
		BruskelCore.Effects.RegisterAdjustment (new SepiaEffect (services));

		// Add the effects
		BruskelCore.Effects.RegisterEffect (new AddNoiseEffect (services));
		BruskelCore.Effects.RegisterEffect (new AlignObjectEffect (services));
		BruskelCore.Effects.RegisterEffect (new BulgeEffect (services));
		BruskelCore.Effects.RegisterEffect (new CellsEffect (services));
		BruskelCore.Effects.RegisterEffect (new CloudsEffect (services));
		BruskelCore.Effects.RegisterEffect (new DentsEffect (services));
		BruskelCore.Effects.RegisterEffect (new DitheringEffect (services));
		BruskelCore.Effects.RegisterEffect (new EdgeDetectEffect (services));
		BruskelCore.Effects.RegisterEffect (new EmbossEffect (services));
		BruskelCore.Effects.RegisterEffect (new FragmentEffect (services));
		BruskelCore.Effects.RegisterEffect (new FrostedGlassEffect (services));
		BruskelCore.Effects.RegisterEffect (new GaussianBlurEffect (services));
		BruskelCore.Effects.RegisterEffect (new GlowEffect (services));
		BruskelCore.Effects.RegisterEffect (new FeatherEffect (services));
		BruskelCore.Effects.RegisterEffect (new OutlineObjectEffect (services));
		BruskelCore.Effects.RegisterEffect (new InkSketchEffect (services));
		BruskelCore.Effects.RegisterEffect (new JuliaFractalEffect (services));
		BruskelCore.Effects.RegisterEffect (new MandelbrotFractalEffect (services));
		BruskelCore.Effects.RegisterEffect (new MedianEffect (services));
		BruskelCore.Effects.RegisterEffect (new MotionBlurEffect (services));
		BruskelCore.Effects.RegisterEffect (new OilPaintingEffect (services));
		BruskelCore.Effects.RegisterEffect (new OutlineEdgeEffect (services));
		BruskelCore.Effects.RegisterEffect (new PencilSketchEffect (services));
		BruskelCore.Effects.RegisterEffect (new PixelateEffect (services));
		BruskelCore.Effects.RegisterEffect (new PolarInversionEffect (services));
		BruskelCore.Effects.RegisterEffect (new RadialBlurEffect (services));
		BruskelCore.Effects.RegisterEffect (new RedEyeRemoveEffect (services));
		BruskelCore.Effects.RegisterEffect (new ReduceNoiseEffect (services));
		BruskelCore.Effects.RegisterEffect (new ReliefEffect (services));
		BruskelCore.Effects.RegisterEffect (new SharpenEffect (services));
		BruskelCore.Effects.RegisterEffect (new SoftenPortraitEffect (services));
		BruskelCore.Effects.RegisterEffect (new TileEffect (services));
		BruskelCore.Effects.RegisterEffect (new TwistEffect (services));
		BruskelCore.Effects.RegisterEffect (new UnfocusEffect (services));
		BruskelCore.Effects.RegisterEffect (new VignetteEffect (services));
		BruskelCore.Effects.RegisterEffect (new VoronoiDiagramEffect (services));
		BruskelCore.Effects.RegisterEffect (new ZoomBlurEffect (services));
	}

	public void Uninitialize ()
	{
		// Remove the adjustments
		BruskelCore.Effects.UnregisterInstanceOfAdjustment<AutoLevelEffect> ();
		BruskelCore.Effects.UnregisterInstanceOfAdjustment<BlackAndWhiteEffect> ();
		BruskelCore.Effects.UnregisterInstanceOfAdjustment<BrightnessContrastEffect> ();
		BruskelCore.Effects.UnregisterInstanceOfAdjustment<CurvesEffect> ();
		BruskelCore.Effects.UnregisterInstanceOfAdjustment<HueSaturationEffect> ();
		BruskelCore.Effects.UnregisterInstanceOfAdjustment<InvertColorsEffect> ();
		BruskelCore.Effects.UnregisterInstanceOfAdjustment<LevelsEffect> ();
		BruskelCore.Effects.UnregisterInstanceOfAdjustment<PosterizeEffect> ();
		BruskelCore.Effects.UnregisterInstanceOfAdjustment<SepiaEffect> ();

		// Remove the effects
		BruskelCore.Effects.UnregisterInstanceOfEffect<AddNoiseEffect> ();
		BruskelCore.Effects.UnregisterInstanceOfEffect<AlignObjectEffect> ();
		BruskelCore.Effects.UnregisterInstanceOfEffect<BulgeEffect> ();
		BruskelCore.Effects.UnregisterInstanceOfEffect<CellsEffect> ();
		BruskelCore.Effects.UnregisterInstanceOfEffect<CloudsEffect> ();
		BruskelCore.Effects.UnregisterInstanceOfEffect<DentsEffect> ();
		BruskelCore.Effects.UnregisterInstanceOfEffect<DitheringEffect> ();
		BruskelCore.Effects.UnregisterInstanceOfEffect<EdgeDetectEffect> ();
		BruskelCore.Effects.UnregisterInstanceOfEffect<EmbossEffect> ();
		BruskelCore.Effects.UnregisterInstanceOfEffect<FragmentEffect> ();
		BruskelCore.Effects.UnregisterInstanceOfEffect<FrostedGlassEffect> ();
		BruskelCore.Effects.UnregisterInstanceOfEffect<GaussianBlurEffect> ();
		BruskelCore.Effects.UnregisterInstanceOfEffect<GlowEffect> ();
		BruskelCore.Effects.UnregisterInstanceOfEffect<FeatherEffect> ();
		BruskelCore.Effects.UnregisterInstanceOfEffect<OutlineObjectEffect> ();
		BruskelCore.Effects.UnregisterInstanceOfEffect<InkSketchEffect> ();
		BruskelCore.Effects.UnregisterInstanceOfEffect<JuliaFractalEffect> ();
		BruskelCore.Effects.UnregisterInstanceOfEffect<MandelbrotFractalEffect> ();
		BruskelCore.Effects.UnregisterInstanceOfEffect<MedianEffect> ();
		BruskelCore.Effects.UnregisterInstanceOfEffect<MotionBlurEffect> ();
		BruskelCore.Effects.UnregisterInstanceOfEffect<OilPaintingEffect> ();
		BruskelCore.Effects.UnregisterInstanceOfEffect<OutlineEdgeEffect> ();
		BruskelCore.Effects.UnregisterInstanceOfEffect<PencilSketchEffect> ();
		BruskelCore.Effects.UnregisterInstanceOfEffect<PixelateEffect> ();
		BruskelCore.Effects.UnregisterInstanceOfEffect<PolarInversionEffect> ();
		BruskelCore.Effects.UnregisterInstanceOfEffect<RadialBlurEffect> ();
		BruskelCore.Effects.UnregisterInstanceOfEffect<RedEyeRemoveEffect> ();
		BruskelCore.Effects.UnregisterInstanceOfEffect<ReduceNoiseEffect> ();
		BruskelCore.Effects.UnregisterInstanceOfEffect<ReliefEffect> ();
		BruskelCore.Effects.UnregisterInstanceOfEffect<SharpenEffect> ();
		BruskelCore.Effects.UnregisterInstanceOfEffect<SoftenPortraitEffect> ();
		BruskelCore.Effects.UnregisterInstanceOfEffect<TileEffect> ();
		BruskelCore.Effects.UnregisterInstanceOfEffect<TwistEffect> ();
		BruskelCore.Effects.UnregisterInstanceOfEffect<UnfocusEffect> ();
		BruskelCore.Effects.UnregisterInstanceOfEffect<VignetteEffect> ();
		BruskelCore.Effects.UnregisterInstanceOfEffect<VoronoiDiagramEffect> ();
		BruskelCore.Effects.UnregisterInstanceOfEffect<ZoomBlurEffect> ();
	}
	#endregion
}

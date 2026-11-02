/////////////////////////////////////////////////////////////////////////////////
// Paint.NET                                                                   //
// Copyright (C) dotPDN LLC, Rick Brewster, Tom Jackson, and contributors.     //
// Portions Copyright (C) Microsoft Corporation. All Rights Reserved.          //
// See third-party/paint.net/LICENSE.md for full licensing and attribution details.             //
//                                                                             //
// Ported to Bruskel by: Jonathan Pobst <monkey@jpobst.com>                      //
/////////////////////////////////////////////////////////////////////////////////

using System;
using Cairo;
using Bruskel.Core;

namespace Bruskel.Effects;

public sealed class InvertColorsEffect : BaseEffect
{
	static readonly UnaryPixelOp invert = new UnaryPixelOps.Invert ();

	public InvertColorsEffect (IServiceProvider _) { }

	public sealed override bool IsTileable
		=> true;

	public override string Icon
		=> Resources.Icons.AdjustmentsInvertColors;

	public override string Name
		=> Translations.GetString ("Invert Colors");

	public override string AdjustmentMenuKey
		=> "I";

	protected override void Render (ImageSurface source, ImageSurface destination, RectangleI roi)
		=> invert.Apply (destination, source, roi);
}

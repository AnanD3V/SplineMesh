# Changelog
All notable changes to this package will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.0.0/),
and this project adheres to [Semantic Versioning](https://semver.org/).

## [1.4.1] - 2025-05-19

### Added
- Added feature to twist the spline mesh based on the rotation of the knots
- Spline mesh can now be twisted by enabling this "Should Twist Mesh" Boolean in the Inspector. Feature available for both SplineMesh and SplineMeshResolution scripts.

### Changed
- Updated `package.json`

## [1.4.0] - 2025-01-27

### Added
- Introduced multiple samples: "Conveyor Belt Sample," "Road Creation Sample," and "Train Tracks Sample."
- Introduced this `CHANGELOG.md` file.

### Changed
- Updated `package.json` to include detailed descriptions for individual samples.

### Removed
- Removed unused materials and assets inside the samples

## [1.3.1] - 2024-01-26

### Added
- Added Samples to the package.

### Fixed
- Fixed 'Auto Generate Mesh' misbehaving inside OnEnable()

### Changed
- Properly formatted `README.md` Instructions.
- Updated `package.json` to include the samples.

## [1.3.0] - 2024-01-26

### Added
- Introduced proper package layout based on Unity's recommendations
- Introduced Spline Collider Generator Scripts - `SplineBoxColliderGenerator.cs` & `SplineCylinderColliderGenerator.cs` to help generate colliders around Spline Mesh. (Mesh Collider)
- Introduced a `SplineMeshUtils.cs` script which centralizes useful Utility functions
- Added Miscellaneous script - `AnimateTextureOffset.cs` to animate textures along the spline
- Added Miscellaneous script - `ConveyorBeltMover.cs` to move Rigidbody objects that rest on top of the SplineMeshes (Useful for Conveyors)
- Introduced proper Assembly Definitions in the project, to improve compile times.
- The package can now be installed directly via Git URL in the Package Manager.
- Introduced Package manifest `package.json` file, and a `README.md` file

### Changed
- Cleaned the code for `SplineMesh.cs` and `SplineMeshResolution.cs`, introducing Tooltips, removing unwanted code, and comments.
- Optimized the Editor Scripts to avoid unnecessary clutter

### Removed
- Removed `SplineFunctions.cs` to introduce the Utility script instead.
- Removed unwanted jargon inside code on all scripts 
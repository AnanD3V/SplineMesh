# Changelog
All notable changes to this package will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.0.0/),
and this project adheres to [Semantic Versioning](https://semver.org/).

## [1.5.0] - 2026-04-14
### Added
- Added feature to save the generated mesh as an FBX file. Just select the GameObject, (at the top) click on SplineMesh > Export Selected to FBX. Added `SplineMeshExporter.cs` file (Editor Menu Item) for this feature. This feature requires FBX Exporter package as a dependency and will be very useful.
- Added feature to save the GameObject as a prefab. Useful if you want to spawn the mesh manually. Select the GameObject, (at the top) click on SplineMesh > Save Selected as Prefab. Added `SplineMeshPrefabSaver.cs` file (Editor Menu Item) for this feature.
- Added URP materials to samples.

### Changed
- Updated `package.json` to include Unity's FBX Exporter package as a dependency. Now this package will be installed automatically when installing through the Package Manager.
- Update `SplineMesh.cs` to check for more null conditions before generating spline. Earlier, when the Spline Container is not null, but contained splines initially, but then if it was removed, would cause the package to break. Now it checks a lot more things before mesh generation.
- Updated `SplineMesh.cs` and `SplineMeshResolution.cs` to include High Density Meshes. This is needed if the generated mesh has more than 65,535 vertices.

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
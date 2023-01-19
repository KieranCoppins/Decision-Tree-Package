# Changelog

All notable changes to this project will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.0.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## Unreleased

### Added

- Colours depending on function data types

### Fixed

- Fixed Function nodes and parameter inputs of mismatching types being able to connect

## [1.1.1] - 2023-01-17

### Fixed

- Fixed being unable to manipulate nodes after leaving readonly mode
- Fixed edges from being editable in readonly mode
- Fixed null exception errors sometimes occuring inbetween play and edit mode
- Fixed function nodes not showing inspector view inside their body when simple node view was false

## [1.1.0] - 2023-01-16

### Added

- Added Action Manager to the component menu
- Added a *simple node view* option in the tool menu. Simple node view displays the tree how it was, turning this off will show the inspector view inside each node and removes the inspector from the side
- Added documentation for the simple node view
- Added Undo/Redo functionality to the creation/deletion of nodes & connections and in changing viewport position/scale
- Added saving viewport position & scale per tree
- Added readonly mode when accessing an instance of a tree. Readonly mode only allows for nodes to be selected and fields inside the inspector to be changed. However, these changes will not carry over to the tree itself as it is only modifying an instance of the tree.

### Changed

- Changed the title of the visual editor when using a custom version to "Decision Tree Editor (Custom)"
- Updated RuntimeVisualisation.md to include the newly added namespace

### Fixed

- Fixed sidebar anchor to be at the edge of the inspector
- Prevented the root node from being deleted / modified (Still draggable)
- Fixed script templates by adding namespaces and missing abstract method overrides

## [1.0.5] - 2023-01-15

### Fixed

- Fixed visual Editor with namespace changes

## [1.0.4] - 2023-01-15

### Added

- Added all runtime code to namespace: `KieranCoppins.DecisionTrees`
- Added all editor code to namespace `KieranCoppins.DecisionTreesEditor`
- Added features to project README

### Changed

- Changed Generic Helpers from 1.0.1 to 1.0.2
- Changed all generic helper reference to use the newly added namespace

## [1.0.3] - 2023-01-15

### Added

- Updated generic helpers to 1.0.1

## [1.0.2] - 2023-01-15

### Fixed

- Banner path in README

## [1.0.1] - 2023-01-15

### Fixed

- Generic helpers dependency name

## [1.0.0] - 2023-01-15
- The first initial release of the Decision Tree Package

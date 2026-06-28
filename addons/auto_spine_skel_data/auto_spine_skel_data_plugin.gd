@tool
extends EditorPlugin

const TARGET_SUFFIX := "_skel_data.tres"
const DEFAULT_MIX := 0.05

var _efs: EditorFileSystem
var _pending_scan := false

func _enter_tree() -> void:
	_efs = get_editor_interface().get_resource_filesystem()
	if _efs:
		if not _efs.filesystem_changed.is_connected(_on_filesystem_changed):
			_efs.filesystem_changed.connect(_on_filesystem_changed)
		if not _efs.resources_reimported.is_connected(_on_resources_reimported):
			_efs.resources_reimported.connect(_on_resources_reimported)
	_request_scan()

func _exit_tree() -> void:
	if _efs:
		if _efs.filesystem_changed.is_connected(_on_filesystem_changed):
			_efs.filesystem_changed.disconnect(_on_filesystem_changed)
		if _efs.resources_reimported.is_connected(_on_resources_reimported):
			_efs.resources_reimported.disconnect(_on_resources_reimported)
	_efs = null

func _on_filesystem_changed() -> void:
	_request_scan()

func _on_resources_reimported(_resources: PackedStringArray) -> void:
	_request_scan()

func _request_scan() -> void:
	if _pending_scan:
		return
	_pending_scan = true
	call_deferred("_scan_and_generate")

func _scan_and_generate() -> void:
	_pending_scan = false
	if not is_instance_valid(_efs):
		return
	var root := _efs.get_filesystem()
	if root:
		_scan_dir(root)

func _scan_dir(dir: EditorFileSystemDirectory) -> void:
	var dir_path := dir.get_path()
	if _is_ignored_dir(dir_path):
		return
	for i in range(dir.get_file_count()):
		var file_name := dir.get_file(i)
		if file_name.ends_with(".skel"):
			_maybe_generate_for_skel(dir_path.path_join(file_name))
	for j in range(dir.get_subdir_count()):
		_scan_dir(dir.get_subdir(j))

func _is_ignored_dir(dir_path: String) -> bool:
	return dir_path.begins_with("res://.godot") or dir_path.begins_with("res://addons")

func _maybe_generate_for_skel(skel_path: String) -> void:
	if _is_ignored_dir(skel_path):
		return
	var base := skel_path.trim_suffix(".skel")
	var atlas_path := base + ".atlas"
	if not FileAccess.file_exists(atlas_path):
		return
	var target_path := base + TARGET_SUFFIX
	if FileAccess.file_exists(target_path):
		return
	var atlas_res := load(atlas_path)
	if atlas_res == null:
		return
	var skel_res := load(skel_path)
	if skel_res == null:
		return
	var data := SpineSkeletonDataResource.new()
	if not _set_if_property_exists(data, &"atlas_res", atlas_res):
		return
	if not _set_if_property_exists(data, &"skeleton_file_res", skel_res):
		return
	_set_if_property_exists(data, &"default_mix", DEFAULT_MIX)
	ResourceSaver.save(data, target_path)

func _set_if_property_exists(obj: Object, prop: StringName, value: Variant) -> bool:
	for p in obj.get_property_list():
		if p.name == prop:
			obj.set(prop, value)
			return true
	return false


extends SceneTree

var packer := PCKPacker.new()
var added := {}
var failures: Array[String] = []

func add_file(res_path: String) -> void:
	if added.has(res_path):
		return
	var absolute_path := ProjectSettings.globalize_path(res_path)
	if not FileAccess.file_exists(absolute_path):
		return
	var err := packer.add_file(res_path, absolute_path)
	if err != OK:
		failures.append("%s (%s)" % [res_path, error_string(err)])
		return
	added[res_path] = true
	if res_path.ends_with(".import"):
		var import_text := FileAccess.get_file_as_string(absolute_path)
		var matcher := RegEx.new()
		matcher.compile('res://[^"\\r\\n]+')
		for match in matcher.search_all(import_text):
			var generated_path := match.get_string()
			if generated_path.begins_with("res://.godot/imported/") or generated_path.begins_with("res://.godot/exported/"):
				add_file(generated_path)

func add_tree(res_directory: String) -> void:
	var directory := DirAccess.open(res_directory)
	if directory == null:
		return
	for file_name in directory.get_files():
		add_file(res_directory.path_join(file_name))
	for directory_name in directory.get_directories():
		add_tree(res_directory.path_join(directory_name))

func _init() -> void:
	var args := OS.get_cmdline_user_args()
	if args.is_empty():
		push_error("Missing output PCK path.")
		quit(2)
		return
	var err := packer.pck_start(args[0])
	if err != OK:
		push_error("Could not start PCK: %s" % error_string(err))
		quit(2)
		return
	add_tree("res://ArtWorks")
	add_tree("res://ChaosHeidemarie")
	add_file("res://mod_manifest.json")
	if not failures.is_empty():
		for failure in failures:
			push_error("Could not add " + failure)
		quit(2)
		return
	err = packer.flush(false)
	if err != OK:
		push_error("Could not finalize PCK: %s" % error_string(err))
		quit(2)
		return
	print("Mod-only PCK completed with %d files: %s" % [added.size(), args[0]])
	quit(0)

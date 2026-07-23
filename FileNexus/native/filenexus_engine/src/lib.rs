use std::ffi::{CStr, CString};
use std::os::raw::{c_char, c_void};
use std::path::Path;
use std::time::UNIX_EPOCH;
use walkdir::WalkDir;

pub type ScanCallbackFn = extern "C" fn(
    name: *const c_char,
    path: *const c_char,
    ext: *const c_char,
    size: u64,
    created_sec: u64,
    modified_sec: u64,
    user_data: *mut c_void,
) -> i32;

/// Scans a directory recursively and invokes the callback for every file found.
/// Returns the total count of scanned files.
#[no_mangle]
pub extern "C" fn filenexus_scan_directory(
    dir_path: *const c_char,
    callback: ScanCallbackFn,
    user_data: *mut c_void,
) -> i64 {
    if dir_path.is_null() {
        return -1;
    }

    let c_str = unsafe { CStr::from_ptr(dir_path) };
    let path_str = match c_str.to_str() {
        Ok(s) => s,
        Err(_) => return -2,
    };

    let target_path = Path::new(path_str);
    if !target_path.exists() || !target_path.is_dir() {
        return -3;
    }

    let mut count: i64 = 0;

    for entry in WalkDir::new(target_path).into_iter().filter_map(|e| e.ok()) {
        let path = entry.path();
        if path.is_file() {
            let metadata = match entry.metadata() {
                Ok(m) => m,
                Err(_) => continue,
            };

            let name_str = entry.file_name().to_string_lossy();
            let full_path_str = path.to_string_lossy();
            let ext_str = path
                .extension()
                .map(|e| e.to_string_lossy())
                .unwrap_or_default();

            let size = metadata.len();
            let created_sec = metadata
                .created()
                .ok()
                .and_then(|t| t.duration_since(UNIX_EPOCH).ok())
                .map(|d| d.as_secs())
                .unwrap_or(0);

            let modified_sec = metadata
                .modified()
                .ok()
                .and_then(|t| t.duration_since(UNIX_EPOCH).ok())
                .map(|d| d.as_secs())
                .unwrap_or(0);

            let c_name = match CString::new(name_str.as_bytes()) {
                Ok(s) => s,
                Err(_) => continue,
            };
            let c_path = match CString::new(full_path_str.as_bytes()) {
                Ok(s) => s,
                Err(_) => continue,
            };
            let c_ext = match CString::new(ext_str.as_bytes()) {
                Ok(s) => s,
                Err(_) => continue,
            };

            let continue_scan = callback(
                c_name.as_ptr(),
                c_path.as_ptr(),
                c_ext.as_ptr(),
                size,
                created_sec,
                modified_sec,
                user_data,
            );

            count += 1;

            if continue_scan == 0 {
                break;
            }
        }
    }

    count
}

/// Frees memory allocated for strings by Rust when returned across FFI.
#[no_mangle]
pub extern "C" fn filenexus_free_string(ptr: *mut c_char) {
    if !ptr.is_null() {
        unsafe {
            let _ = CString::from_raw(ptr);
        }
    }
}

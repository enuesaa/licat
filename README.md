# licat
toyapp. browse files and copy their contents.

## Usage
```bash
$ licat --help
Description:
  licat --- browse files and copy their contents

Usage:
  licat [options]

Options:
  -h, --help  Show help and usage information
  --version   Show version information
```

## Feature Plan
- add `--print-all` flag to stdout file contents
- add `--resume` flag to re-copy previous copied files
  - saves `~/.licat/resume.json`

```json
{
  "paths": {
    "/Users/aaa/licat": {
      "copied": ["Program.cs", "Menu.cs", "FileViewer.cs"]
    }
  }
}
```

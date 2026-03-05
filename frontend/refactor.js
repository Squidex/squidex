#!/usr/bin/env node

const fs = require("fs");
const path = require("path");

const ROOT_DIR = ".";
const SKIP_DIRS = ["node_modules", ".git", "dist", "build", ".cache"];
const BUTTON_TAG_RE = /<button(\s[^>]*?)?>/gi;

function fixButtons(html) {
  return html.replace(BUTTON_TAG_RE, (match, attrs) => {
    if (attrs && /\btype\s*=/i.test(attrs)) return match;
    return attrs ? `<button type="button"${attrs}>` : `<button type="button">`;
  });
}

function walk(dir) {
  for (const entry of fs.readdirSync(dir, { withFileTypes: true })) {
    const fullPath = path.join(dir, entry.name);

    if (entry.isDirectory()) {
      if (!SKIP_DIRS.includes(entry.name)) walk(fullPath);
    } else if (entry.isFile() && entry.name.endsWith(".component.html")) {
      const original = fs.readFileSync(fullPath, "utf8");
      const fixed = fixButtons(original);

      if (fixed !== original) {
        fs.writeFileSync(fullPath, fixed, "utf8");
        console.log(`Fixed: ${fullPath}`);
      }
    }
  }
}

walk(path.resolve(ROOT_DIR));
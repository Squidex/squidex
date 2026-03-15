#!/usr/bin/env node

const fs = require("fs");
const path = require("path");

class AddButtonTypesStrategy {
    #buttonTagPattern = /<button(\s[^>]*?)?>/gi;

    fix(html) {
        return html.replace(this.#buttonTagPattern, (match, attributes) => {
            if (attributes && /\btype\s*=/i.test(attributes)) {
                return match;
            }
            return attributes ? `<button type="button"${attributes}>` : `<button type="button">`;
        });
    }
}

class FixIconOnlyInteractivesStrategy {
    #interactiveTagPattern = /<(a|button)(\s[^>]*)?>/gi;
    #iconPattern = /<i class="icon[^>]*>[\s\S]*?<\/i>/gi;
    #titleAttributePattern = /\btitle\s*=\s*["']([^"']*)["']/i;
    #screenReaderPattern = /visually-hidden|sr-only|cdk-visually-hidden|aria-label\s*=|aria-labelledby\s*=/i;

    #resolveTitle(title) {
        if (title.startsWith("i18n:")) {
            return `{{ '${title.replace("i18n:", "i18n.")}' | sqxTranslate }}`;
        }
        return title;
    }

    #visuallyHiddenSpan(title) {
        return `<span class="visually-hidden">${this.#resolveTitle(title)}</span>`;
    }

    #findClosingTag(html, from, tag) {
        const openingTagPattern = new RegExp(`<${tag}[\\s>]`, "gi");
        const closingTagPattern = new RegExp(`<\\/${tag}\\s*>`, "gi");
        let depth = 1;
        let position = from;

        while (depth > 0) {
            openingTagPattern.lastIndex = position;
            closingTagPattern.lastIndex = position;
            const nextOpening = openingTagPattern.exec(html);
            const nextClosing = closingTagPattern.exec(html);

            if (!nextClosing) {
                return html.length;
            }

            if (nextOpening && nextOpening.index < nextClosing.index) {
                depth++;
                position = nextOpening.index + 1;
            } else {
                depth--;
                position = nextClosing.index + nextClosing[0].length;
            }
        }
        return position;
    }

    #lineAndColumn(html, index) {
        const before = html.slice(0, index);
        const line = (before.match(/\n/g) || []).length + 1;
        const column = index - before.lastIndexOf("\n");
        return { line, column };
    }

    #hasOnlyIcons(inner) {
        this.#iconPattern.lastIndex = 0;
        const withoutIcons = inner.replace(this.#iconPattern, "").replace(/<[^>]+>/g, "").trim();
        this.#iconPattern.lastIndex = 0;
        return withoutIcons.length === 0 && this.#iconPattern.test(inner);
    }

    fix(html, filePath) {
        const fixes = [];
        let match;

        this.#interactiveTagPattern.lastIndex = 0;

        while ((match = this.#interactiveTagPattern.exec(html)) !== null) {
            const tag = match[1];
            const attributes = match[2] || "";
            const openEnd = match.index + match[0].length;
            const closeEnd = this.#findClosingTag(html, openEnd, tag);
            const inner = html.slice(openEnd, closeEnd - `</${tag}>`.length);

            if (!this.#hasOnlyIcons(inner) || this.#screenReaderPattern.test(inner) || this.#screenReaderPattern.test(attributes)) {
                continue;
            }

            const titleMatch = this.#titleAttributePattern.exec(attributes);

            const { line, column } = this.#lineAndColumn(html, match.index);

            if (titleMatch) {
                fixes.push({ insertAt: openEnd + inner.length, title: titleMatch[1] });
            } else {
                fixes.push({ insertAt: openEnd + inner.length, title: null });
                console.log(`${filePath}:${line}:${column}`);
            }
        }

        let result = html;
        for (const fix of fixes.sort((a, b) => b.insertAt - a.insertAt)) {
            const span = fix.title !== null
                ? this.#visuallyHiddenSpan(fix.title)
                : this.#visuallyHiddenSpan("{{ 'i18n:TODO' | sqxTranslate }}</span>");
            result = result.slice(0, fix.insertAt) + span + result.slice(fix.insertAt);
        }

        return result;
    }
}

const ROOT_DIR = ".";
const SKIP_DIRS = ["node_modules", ".git", "dist", "build", ".cache"];
const SKIP_FILES = ["_theme.html"];

const strategies = [
    new AddButtonTypesStrategy(),
    new FixIconOnlyInteractivesStrategy(),
];

function walk(dir) {
    for (const entry of fs.readdirSync(dir, { withFileTypes: true })) {
        const fullPath = path.join(dir, entry.name);

        if (entry.isDirectory()) {
            if (!SKIP_DIRS.includes(entry.name)) {
                walk(fullPath);
            }
        } else if (entry.isFile() && entry.name.endsWith(".component.html") && !SKIP_FILES.includes(entry.name)) {
            let content = fs.readFileSync(fullPath, "utf8");
            let changed = false;

            for (const strategy of strategies) {
                const updated = strategy.fix(content, fullPath);
                if (updated !== content) {
                    content = updated;
                    changed = true;
                }
            }

            if (changed) {
                fs.writeFileSync(fullPath, content, "utf8");
            }
        }
    }
}

walk(path.resolve(ROOT_DIR));
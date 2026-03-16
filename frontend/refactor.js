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
    #visuallyHiddenSpanPattern = /\s*<span\s+class="visually-hidden">([\s\S]*?)<\/span>/i;
    #nestedInteractiveOpenPattern = /<(a|button)[\s>]/gi;
    #nestedInteractiveClosePattern = /<\/(a|button)\s*>/gi;

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

    #isDirectChild(inner, spanIndex) {
        let depth = 0;
        let position = 0;

        while (position < spanIndex) {
            this.#nestedInteractiveOpenPattern.lastIndex = position;
            this.#nestedInteractiveClosePattern.lastIndex = position;
            const nextOpening = this.#nestedInteractiveOpenPattern.exec(inner);
            const nextClosing = this.#nestedInteractiveClosePattern.exec(inner);

            const openingBeforeSpan = nextOpening && nextOpening.index < spanIndex;
            const closingBeforeSpan = nextClosing && nextClosing.index < spanIndex;

            if (openingBeforeSpan && (!closingBeforeSpan || nextOpening.index < nextClosing.index)) {
                depth++;
                position = nextOpening.index + 1;
            } else if (closingBeforeSpan) {
                depth--;
                position = nextClosing.index + 1;
            } else {
                break;
            }
        }

        return depth === 0;
    }

    fix(html) {
        const fixes = [];
        let match;

        this.#interactiveTagPattern.lastIndex = 0;

        while ((match = this.#interactiveTagPattern.exec(html)) !== null) {
            const tag = match[1];
            const attributes = match[2] || "";
            const openEnd = match.index + match[0].length;
            const closeEnd = this.#findClosingTag(html, openEnd, tag);
            const inner = html.slice(openEnd, closeEnd - `</${tag}>`.length);
            const spanMatch = this.#visuallyHiddenSpanPattern.exec(inner);

            if (!spanMatch || !this.#isDirectChild(inner, spanMatch.index)) {
                continue;
            }

            const ariaLabel = spanMatch[1].trim().replace(/"/g, "'");
            const innerWithout = inner.slice(0, spanMatch.index) + inner.slice(spanMatch.index + spanMatch[0].length);
            const newOpenTag = `<${tag}${attributes} attr.aria-label="${ariaLabel}">`;

            fixes.push({
                from: match.index,
                to: openEnd + inner.length,
                newOpenTag,
                newInner: innerWithout,
            });
        }

        let result = html;
        for (const fix of fixes.sort((a, b) => b.from - a.from)) {
            result = result.slice(0, fix.from) + fix.newOpenTag + fix.newInner + result.slice(fix.to);
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
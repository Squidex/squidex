// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Squidex.Infrastructure.Json.Objects;
using Squidex.Infrastructure.ObjectPool;
using Squidex.Text.RichText;
using Squidex.Text.RichText.Model;

namespace Squidex.Domain.Apps.Core.Contents;

public sealed class RichTextNode : INode
{
    private readonly RichTextMark mark = new RichTextMark();
    private State currentState;

    internal struct State
    {
        public JsonObject? Root;
        public NodeType Type;
        public JsonArray? Marks;
        public JsonObject? Attrs;
        public JsonArray? Content;
        public string? Text;
        public int MarkIndex;
    }

    public NodeType Type
    {
        get => currentState.Type;
    }

    public JsonObject? Root
    {
        get => currentState.Root;
    }

    public string? Text
    {
        get => currentState.Text;
    }

    public static bool TryCreate(JsonValue source, out RichTextNode node)
    {
        var candidate = new RichTextNode();

        if (candidate.TryUse(source, true))
        {
            node = candidate;
            return true;
        }

        node = null!;
        return false;
    }

    public static RichTextNode Create(JsonValue source)
    {
        var node = new RichTextNode();

        // We assume that we have made the validation before.
        node.TryUse(source, false);

        return node;
    }

    public bool TryUse(JsonValue source, bool recursive = false)
    {
        State state = default;

        if (source.Value is not JsonObject obj)
        {
            currentState = state;
            return false;
        }

        state.Root = obj;

        var isValid = true;
        foreach (var (key, value) in obj)
        {
            switch (key)
            {
                case "type" when value.TryGetEnum<NodeType>(out var type):
                    state.Type = type;
                    break;
                case "attrs" when value.Value is JsonObject attrs:
                    state.Attrs = attrs;
                    break;
                case "marks" when value.TryGetArrayOfObject(out var marks):
                    state.Marks = marks;
                    break;
                case "content" when value.TryGetArrayOfObject(out var content):
                    state.Content = content;
                    break;
                case "text" when value.Value is string text:
                    state.Text = text;
                    break;
            }
        }

        currentState = state;

        isValid &= Type != NodeType.Undefined;

        if (isValid && recursive)
        {
            if (state.Content != null)
            {
                foreach (var content in state.Content)
                {
                    // We have already validated this before.
                    isValid &= TryUse((JsonObject)content.Value!, recursive);
                }
            }

            if (state.Marks != null)
            {
                foreach (var markObj in state.Marks)
                {
                    // We have already validated this before.
                    isValid &= mark.TryUse((JsonObject)markObj.Value!);
                }
            }
        }

        currentState = state;

        return isValid;
    }

    public int GetIntAttr(string name, int defaultValue = 0)
    {
        return currentState.Attrs.GetIntAttr(name, defaultValue);
    }

    public string GetStringAttr(string name, string defaultValue = "")
    {
        return currentState.Attrs.GetStringAttr(name, defaultValue);
    }

    public IMark? GetNextMark()
    {
        if (currentState.Marks == null || currentState.MarkIndex >= currentState.Marks.Count)
        {
            return null;
        }

        // We have already validated this before.
        mark.TryUse((JsonObject)currentState.Marks[currentState.MarkIndex++].Value!);
        return mark;
    }

    public void IterateContent<T>(T state, Action<INode, T, bool, bool> action)
    {
        var prevState = currentState;

        if (prevState.Content == null)
        {
            return;
        }

        var i = 0;
        foreach (var item in prevState.Content)
        {
            var isFirst = i == 0;
            var isLast = i == prevState.Content.Count - 1;

            // We have already validated this before.
            TryUse((JsonObject)item.Value!, false);
            action(this, state, isFirst, isLast);
            i++;
        }

        currentState = prevState;
    }

    public string ToMarkdown()
    {
        var sb = DefaultPools.StringBuilder.Get();
        try
        {
            MarkdownVisitor.Render(this, sb);
            return sb.ToString();
        }
        finally
        {
            DefaultPools.StringBuilder.Return(sb);
        }
    }

    public string ToHtml(int indentation = 4)
    {
        var sb = DefaultPools.StringBuilder.Get();
        try
        {
            HtmlWriterVisitor.Render(this, sb, new HtmlWriterOptions { Indentation = indentation });
            return sb.ToString();
        }
        finally
        {
            DefaultPools.StringBuilder.Return(sb);
        }
    }

    public string ToText(int maxLength = int.MaxValue)
    {
        var sb = DefaultPools.StringBuilder.Get();
        try
        {
            TextVisitor.Render(this, sb, maxLength);
            return sb.ToString();
        }
        finally
        {
            DefaultPools.StringBuilder.Return(sb);
        }
    }
}

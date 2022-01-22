import { h } from 'preact';
import { useEffect, useState, useMemo, useRef, useLayoutEffect } from 'preact/hooks';
import { TokenInfo } from './shared';

export interface OverlayProps {
    // The target element ot attach to.
    target: HTMLElement;

    // The token string.
    token: TokenInfo;

    // When opened.
    onOpen: (url: string) => void;
}

const PADDING = 4;

export const Overlay = (props: OverlayProps) => {
    const { onOpen, target, token } = props;
    const linksRef = useRef<HTMLDivElement>();
    const linksRect = useRef<DOMRect>();
    const targetRect = useRef<DOMRect>();
    const [_, render] = useState(0);

    useEffect(() => {
        render(x => x + 1);
    }, [token]);

    useEffect(() => {
        function layout() {
            const rect = target.getBoundingClientRect();

            const current = targetRect.current;

            if (!current ||
                rect.height !== current.height ||
                rect.width !== current.width ||
                rect.x !== current.x ||
                rect.y !== current.y) {
                targetRect.current = rect;
                render(x => x + 1);
            }
        }

        document.body.addEventListener('scroll', layout);

        const timer = setInterval(() => {
            layout();
        }, 500);

        layout();

        return () => {
            clearInterval(timer);

            document.body.removeEventListener('scroll', layout);
        };
    }, [target]);

    useLayoutEffect(() => {
        if (!linksRef.current) {
            return;
        }

        const rect = linksRef.current.getBoundingClientRect();

        const current = linksRect.current;

        if (!current ||
            rect.height !== current.height ||
            rect.width !== current.width) {
            linksRect.current = rect;
            render(x => x + 1);
        }
    }, [render]);

    const externalUrl = useMemo(() => {
        let { a, i, s, u } = token;

        if (s) {
            return `${u}/app/${a}/content/${s}/${i}`;
        } else {
            return `${u}/app/${a}/assets/?ref=${i}`;
        }
    }, [token]);

    const embedUrl = useMemo(() => {
        let { a, i, s, u } = token;

        if (s) {
            return `${u}/embed/app/${a}/content/${s}/${i}`;
        } else {
            return `${u}/embed/app/${a}/assets/?ref=${i}`;
        }
    }, [token]);

    const x = (targetRect.current?.x || 0) - PADDING;
    const y = (targetRect.current?.y || 0) - PADDING;

    const overlayWidth = (targetRect.current?.width || 0) + 2 * PADDING;
    const overlayHeight = (targetRect.current?.height || 0) + 2 * PADDING;

    const linksWidth = (linksRect.current?.width || 0);
    const linksHeight = (linksRect.current?.height || 0);

    let linkX = x;
    let linkY = y - linksHeight;

    if (linkY < 0) {
        linkY = y + overlayHeight;
    }

    if (linkY + linksHeight > window.innerHeight) {
        linkX = window.innerHeight - linksHeight;
    }

    if (linkX < 0) {
        linkX = 0;
    }

    if (linkX + linksWidth > window.innerWidth) {
        linkX = window.innerWidth - linksWidth;
    }

    return (
        <div class='squidex-overlay' style={{ left: x, top: y, width: overlayWidth, height: overlayHeight }}>
            <div class='squidex-overlay-toolbar' style={{ left: linkX, top: linkY }} ref={linksRef as any}>
                <div class='squidex-overlay-schema'>
                    {token?.s || 'Asset'}
                </div>

                <div class='squidex-overlay-links'>
                    <a onClick={() => onOpen(embedUrl)}>
                        Edit Inline
                    </a>

                    <a href={externalUrl!} target='_blank'>
                        Edit In Squidex
                    </a>
                </div>
            </div>
        </div>
    );
};
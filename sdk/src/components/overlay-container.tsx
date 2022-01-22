import { h } from 'preact';
import { useCallback, useEffect, useRef, useState } from 'preact/hooks';
import { IFrame } from './iframe';
import { Overlay } from './Overlay';
import { TokenInfo } from './shared';

export const OverlayContainer = () => {
    const div = useRef<any>();
    const [auth, setAuth] = useState<{ [url: string]: Boolean }>({});
    const [target, setTarget] = useState<{ target: HTMLElement, token: TokenInfo}>();
    const [targetUrl, setTargetUrl] = useState<string>();
    const [uniqueUrls, setUniqueUrls] = useState<{ [url: string]: string }>({});
    
    useEffect(() => {
        let previous: any;
        let previousTarget: HTMLElement | null = null;

        function listen(event: MouseEvent) {
            const target = event.target as HTMLElement;

            if (target && target !== previous) {
                try {
                    const token = parseToken(target);
    
                    if (token) {
                        previousTarget = target;
                        
                        setTarget({ target, token });
                    } else if (previousTarget && !previousTarget.contains(target) && !div.current?.contains(target)) {
                        previousTarget = null;
    
                        setTarget(undefined);
                    }
                } catch {
                }
    
                previous = target;
            }
        }

        document.addEventListener('mousemove', listen);

        return () => {
            document.removeEventListener('mousemove', listen);
        }
    }, []);

    useEffect(() => {
        const url = target?.token.u;

        if (!url) {
            return;
        }

        if (url.indexOf('http://') >= 0) {
            setAuth(auth => ({ ...auth, [url]: true }));
            return;
        }

        setUniqueUrls(urls => {
            if (urls[url]) {
                return urls;
            } else {
                const image = `${url}/identity-server/status.png`;

                return { ...uniqueUrls, [url]: image };
            }
        });
    }, [target?.token.u]);

    const doAuth = useCallback((url: string, status: boolean) => {
        setAuth(auth => ({ ...auth, [url]: status }));
    }, []);

    return (
        <div class='squidex' ref={div}>
            {target && auth[target.token.u!] === true &&
                <Overlay onOpen={setTargetUrl} {...target} />
            }

            {Object.entries(uniqueUrls).map(([u, img]) => 
                <img width={0} height={0} key={img} src={img} onLoad={() => doAuth(u, true)} onError={() => doAuth(u, false)} />    
            )}

            {targetUrl &&
                <IFrame url={targetUrl} onClose={() => setTargetUrl(undefined)} />
            }
        </div>
    );
}

function parseToken(target: HTMLElement): TokenInfo | null {
    const value = target.getAttribute('squidex-token');

    if (!value) {
        return null;
    }
    
    try {
        const decoded = atob(value);

        let token = JSON.parse(decoded) as TokenInfo;

        if (!token.u || !token.i || !token.a) {
            return null;
        }

        while (token.u.endsWith('/')) {
            token.u = token.u.substring(0, token.u.substring.length - 1);
        }

        return token;
    } catch {
        return null;
    }
}
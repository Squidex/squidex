import { h } from 'preact';
import { useCallback, useEffect, useRef, useState } from 'preact/hooks';
import { IFrame } from './iframe';
import { Overlay } from './Overlay';
import { TokenInfo } from './shared';

export interface OverlayContainerProps {
    // The base url of the script.
    baseUrl: string | null | undefined;
}

type AuthState = 'Authenticated' | 'Failed' | 'Pending';

export const OverlayContainer = (props: OverlayContainerProps) => {
    const div = useRef<any>();
    const [auth, setAuth] = useState<{ [url: string]: AuthState }>({});
    const [target, setTarget] = useState<{ target: HTMLElement, token: TokenInfo}>();
    const [targetUrl, setTargetUrl] = useState<string>();
    const authRef = useRef(auth);
    
    useEffect(() => {
        let previous: any;
        let previousTarget: HTMLElement | null = null;

        function listen(event: MouseEvent) {
            const target = event.target as HTMLElement;

            if (target && target !== previous) {
                try {
                    const token = parseToken(target, Object.keys(authRef.current));
    
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

    const checkAuth = useCallback((url: string | null | undefined) => {
        if (!url) {
            return;
        }

        if (authRef.current[url]) {
            return;
        }

        const setStatus = (state: AuthState) => {
            const newAuth = {
                ...authRef.current,
                [url]: state
            };

            authRef.current = newAuth;

            setAuth(newAuth);
        };

        if (url.indexOf('http://') === 0) {
            setStatus('Authenticated');
            return;
        }

        const fetchStatus = async () => {
            setStatus('Pending');

            try {
                const response = await fetch(`${url}/identity-server/info`, {
                    credentials: 'include'
                });

                const json = await response.json();

                setStatus(json.displayName ? 'Authenticated' : 'Failed');
            } catch {
                setStatus('Failed');
            }
        };

        fetchStatus();
    }, [auth]);

    useEffect(() => {
        checkAuth(props.baseUrl);
    }, [props.baseUrl]);

    useEffect(() => {
        checkAuth(target?.token.u);
    }, [target?.token.u]);
    
    const isAuthenticated = auth[target?.token.u!] === 'Authenticated'

    return (
        <div class='squidex' ref={div}>
            {target && isAuthenticated &&
                <Overlay onOpen={setTargetUrl} {...target} />
            }

            {targetUrl &&
                <IFrame url={targetUrl} onClose={() => setTargetUrl(undefined)} />
            }
        </div>
    );
}

const CDN_URL = 'https://assets.squidex.io';

function parseToken(target: HTMLElement, baseUrls: string[]): TokenInfo | null {
    const value = target.getAttribute('squidex-token');

    if (!value && target.nodeName === 'IMG') {
        const src = (target as any)['src'] as string;

        if (src) {
            for (const baseUrl of baseUrls) {
                if (src.indexOf(baseUrl) === 0) {
                    const parts = src.substring(baseUrl.length + 1).split('/');

                    if (parts[0] === 'api' &&
                        parts[1] === 'assets' &&
                        parts[2]?.length > 0 &&
                        parts[3]?.length > 0) {
                        return {
                            u: baseUrl,
                            a: parts[2],
                            i: parts[3]
                        };
                    }
                }
            }

            if (src.indexOf(CDN_URL) === 0) {
                const parts = src.substring(CDN_URL.length + 1).split('/');

                if (parts[0]?.length > 0 &&
                    parts[1]?.length > 0) {
                    return {
                        u: CDN_URL,
                        a: parts[0],
                        i: parts[1]
                    };
                }

            }
        }
    }

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
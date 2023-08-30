let testAnchor: HTMLAnchorElement;
export function toAbsoluteUri(relativeUri: string) {
  if (self.document) {
    testAnchor = testAnchor || self.document.createElement("a");
    testAnchor.href = relativeUri;
    return testAnchor.href;
  } else {
    return `${self.location.origin}/${relativeUri}`;
  }
}

export async function responseProgress(
  response: Response,
  progressCallback: (progress: number) => void
): Promise<Response> {
  if (!response.ok) {
    throw Error(`${response.status} ${response.statusText}`);
  }

  try {
    // Wrap original stream with another one, while reporting progress.
    const stream = new ReadableStream({
      start: (controller) => {
        if (response.body) {
          const reader = response.body.getReader();
          streamWithProgress(reader, controller, progressCallback);
        }
      },
    });

    // We copy the previous response to keep original headers.
    // Not only the WebAssembly will require the right content-type,
    // but we also need it for streaming optimizations:
    // https://bugs.chromium.org/p/chromium/issues/detail?id=719172#c28
    return new Response(stream, response);
  } catch (ex) {
    // ReadableStream may not be supported (Edge as of 42.17134.1.0)
    return response;
  }
}

async function streamWithProgress(
  reader: ReadableStreamDefaultReader<Uint8Array>,
  ctl: ReadableStreamDefaultController<any>,
  progressCallback: (progress: number) => void
): Promise<void> {
  try {
    do {
      var result = await reader.read();

      if (result.value) {
        progressCallback(result.value.byteLength);
        ctl.enqueue(result.value);
      }
    } while (!result.done);

    ctl.close();
  } catch (e) {
    console.error(e);
    ctl.error(e);
  }
}

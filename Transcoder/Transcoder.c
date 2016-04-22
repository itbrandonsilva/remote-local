// This is the main DLL file.

//#include "extras/stdint.h"
//#include "extras/inttypes.h"
#include "stdint.h"
#include "x264.h"
#include <libavcodec/avcodec.h>
#include <libswscale/swscale.h>
#include <libavutil/imgutils.h>

#include <time.h>

x264_param_t param;

int sz;
uint8_t* bts;

char* NALBYTES;

x264_picture_t pic_in;

AVFrame* av_frame;
AVFrame* av_frame_rgba;

struct SwsContext* convert_context;
struct SwsContext* convertCtx;

const int HEADER_SIZE = sizeof(int) * 2;
enum PACKET_TYPE {
	VIDEO_FRAME = 3
};


struct TranscoderOptions {
	int InputWidth;
	int InputHeight;
	int OutputWidth;
	int OutputHeight;
};

struct TranscoderContext {
	x264_t* encoder;
	AVCodecContext* decoder_ctx;
	struct TranscoderOptions* options;
};

void FreeEncoder(x264_t* encoder) {
	x264_encoder_close(encoder);
	sws_freeContext(convertCtx);
}

void FreeFfmpeg(AVCodecContext* ctx) {
	avcodec_close(ctx);
	av_frame_free(&av_frame_rgba);
	av_frame_free(&av_frame);
	sws_freeContext(convert_context);
}

_declspec(dllexport) struct TranscoderContext* __cdecl AllocContext(struct TranscoderOptions* options) {
	struct TranscoderContext* ctx = malloc(sizeof(struct TranscoderContext));
	ctx->options = options;
	return ctx;
}

_declspec(dllexport) int __cdecl AllocEncoder(struct TranscoderContext* ctx) {
	struct TranscoderOptions* options = ctx->options;

	if (x264_picture_alloc(&pic_in, X264_CSP_I420, options->OutputWidth, options->OutputHeight) < 0) {
		return 1;
	}

	fprintf(stdout, "Width: %d\n", options->InputWidth);
	fprintf(stdout, "Height: %d\n", options->InputHeight);

	NALBYTES = malloc(options->OutputWidth * options->OutputHeight * 4);

	x264_param_t param;
	x264_param_default_preset(&param, "ultrafast", "zerolatency");
	param.i_threads = 0;
	param.i_width = options->OutputWidth;
	param.i_height = options->OutputHeight;
	param.rc.i_rc_method = X264_RC_CRF;

	//param.rc.i_bitrate = 3000;
	//param.rc.i_vbv_max_bitrate = 4000;
	param.rc.f_rf_constant = 20;
	fprintf(stdout, "rf_constant: %f\n", param.rc.f_rf_constant);
	fprintf(stdout, "rf_constant_max: %f\n", param.rc.f_rf_constant_max);
	//fprintf(stdout, "rf_constant_max: %f\n", param.rc.rf);

	//param.i_keyint_max = 25;
	param.i_keyint_max = 500;

	//param.b_intra_refresh = 1;
	param.b_repeat_headers = 1;
	param.b_annexb = 1;
	//param.i_log_level = -1;

	x264_t* encoder = x264_encoder_open(&param);
	ctx->encoder = encoder;

	convertCtx = sws_getContext(options->InputWidth, options->InputHeight, AV_PIX_FMT_RGBA, options->OutputWidth, options->OutputHeight, AV_PIX_FMT_YUV420P, SWS_FAST_BILINEAR, NULL, NULL, NULL);

	return 0;
}

_declspec(dllexport) int __cdecl AllocDecoder(struct TranscoderContext* ctx) {
	av_frame = av_frame_alloc();
	if (!av_frame) {
		fprintf(stderr, "Could not allocate video frame\n");
		exit(1);
	}
	//av_frame->format = av_codec_context->pix_fmt;
	//av_frame->format = AV_PIX_FMT_YUV420P;
	//av_frame->width = width;
	//av_frame->height = height;
	//size = av_image_alloc(av_frame->data, av_frame->linesize, av_codec_context->width, av_codec_context->height, av_codec_context->pix_fmt, 32);
	//size = av_image_alloc(av_frame->data, av_frame->linesize, width, height, AV_PIX_FMT_YUV420P, 32);
	//if (size < 0) {
	//	fprintf(stderr, "Could not allocate raw picture buffer\n");
	//	exit(1);
	//}

	av_frame_rgba = av_frame_alloc();
	if (!av_frame_rgba) {
		fprintf(stderr, "Could not allocate video frame\n");
		exit(1);
	}
	//av_frame_rgba->format = av_codec_context->pix_fmt;
	//av_frame->format = AV_PIX_FMT_RGBA;
	//av_frame_rgba->width = width;
	//av_frame_rgba->height = height;
	//size = av_image_alloc(av_frame_rgba->data, av_frame_rgba->linesize, av_codec_context->width, av_codec_context->height, AV_PIX_FMT_RGBA, 32);
	//size = av_image_alloc(av_frame_rgba->data, av_frame_rgba->linesize, width, height, AV_PIX_FMT_RGBA, 32);
	//if (size < 0) {
	//	fprintf(stderr, "Could not allocate raw picture buffer\n");
	//	exit(1);
	//}

	struct TranscoderOptions* options = ctx->options;
	sz = options->OutputWidth * options->OutputHeight * 4;
	bts = (uint8_t*)malloc(sz);

	AVCodecContext* av_codec_context;
	AVCodec* av_codec;

	avcodec_register_all();

	av_codec = avcodec_find_decoder(AV_CODEC_ID_H264);
	if (!av_codec) {
		fprintf(stderr, "Codec not found\n");
		exit(1);
	}

	av_codec_context = avcodec_alloc_context3(av_codec);
	av_codec_context->width = options->OutputWidth;
	av_codec_context->height = options->OutputHeight;
	//av_codec_context->extradata = NULL;
	av_codec_context->pix_fmt = AV_PIX_FMT_YUV420P;

	if (avcodec_open2(av_codec_context, av_codec, NULL) < 0) {
		fprintf(stderr, "Could not open codec\n");
		exit(1);
	}

	convert_context = sws_getContext(options->OutputWidth, options->OutputHeight, AV_PIX_FMT_YUV420P, options->OutputWidth, options->OutputHeight, AV_PIX_FMT_RGBA, SWS_FAST_BILINEAR, NULL, NULL, NULL);

	av_codec_context->thread_count = 12;
	fprintf(stdout, "Thread Count: %d\n", av_codec_context->thread_count);

	ctx->decoder_ctx = av_codec_context;

	return 0;
}

_declspec(dllexport) void __cdecl FreeContext(struct TranscoderContext* ctx) {
	FreeEncoder(ctx->encoder);
	FreeFfmpeg(ctx->decoder_ctx);
	x264_picture_clean(&pic_in);
	free(bts);
	free(ctx);
}

_declspec(dllexport) void __cdecl TheInit(int width, int height, int fps) {

}

_declspec(dllexport) void __cdecl Free() {

}

_declspec(dllexport) int __cdecl EncodeFrame(struct TranscoderContext* ctx, char* bgraInput, char* packetOutput) {
	//fprintf(stdout, "------------------------\n");
	clock_t begin = clock();
	//TheInit(width, height, fps);
	//x264_t* encoder = AllocEncoder(width, height, fps);
	//AVCodecContext* av_codec_context = AllocFfmpeg(width, height);

	x264_t* encoder = ctx->encoder;

	struct TranscoderOptions* options = ctx->options;
	int iWidth = options->InputWidth;
	int iHeight = options->InputHeight;
	int oWidth = options->OutputWidth;
	int oHeight = options->OutputHeight;

	int size = 0;
	int result = 0;

	//struct SwsContext* convertCtx = sws_getContext(iWidth, iHeight, AV_PIX_FMT_RGBA, oWidth, oHeight, AV_PIX_FMT_YUV420P, SWS_FAST_BILINEAR, NULL, NULL, NULL);
	if (!convertCtx) {
		return -1;
	}

	//data is a pointer to you RGBA structure
	int srcstride = iWidth * 4; //RGBA stride is just 4*width
	sws_scale(convertCtx, &bgraInput, &srcstride, 0, iHeight, pic_in.img.plane, pic_in.img.i_stride);
	//sws_freeContext(convertCtx);

	x264_nal_t* nals;
	int i_nals;
	x264_picture_t pic_out;
	int frame_size = x264_encoder_encode(encoder, &nals, &i_nals, &pic_in, &pic_out);

	int i;
	int apparentSize = 0;
	//char* NALBYTES = malloc(width*height * 4);
	if (frame_size >= 0)
	{
		int index = HEADER_SIZE;
		//index = 0;
		for (i = 0; i < i_nals; i++)
		{
			x264_nal_t nal = nals[i];
			//memcpy(&(NALBYTES[index]), nal.p_payload, nal.i_payload);
			memcpy(&(packetOutput[index]), nal.p_payload, nal.i_payload);
			index += nal.i_payload;
			apparentSize += nal.i_payload;
		}

		//fprintf(stdout, "SIZE: %d\n", apparentSize);
		int type = VIDEO_FRAME;
		memcpy(packetOutput, &type, sizeof(int));
		memcpy(&(packetOutput[sizeof(int)]), &apparentSize, sizeof(int));

		//memcpy(packetOutput, &apparentSize, sizeof(int));
	}

	int packetSize = apparentSize + HEADER_SIZE;
	return packetSize;
}

//_declspec(dllexport) void __cdecl DecodeFrame(struct TranscoderContext* ctx, char* bytes, void* out) {
_declspec(dllexport) void __cdecl DecodeFrame(struct TranscoderContext* ctx, char* packetInput, char* bgraOutput) {
	AVCodecContext* av_codec_context = ctx->decoder_ctx;
	struct TranscoderOptions* options = ctx->options;
	//int iWidth = options->InputWidth;
	//int iHeight = options->InputHeight;
	int oWidth = options->OutputWidth;
	int oHeight = options->OutputHeight;
	//fprintf(stdout, "Apparent size: %d\n", apparentSize);


	//apparentSize = sizeof(uint32_t) * 2 + i;

	//x264_picture_clean(&pic_in);

	//free(NALBYTES);
	//return;

	// First 8 bytes contain the header. The latter 4 bytes contain the size of the frame
	int apparentSize = 0;
	memcpy(&apparentSize, &(packetInput[sizeof(int)]), sizeof(int));
	//memcpy(&apparentSize, packetInput, sizeof(int));


	//AVFrame* av_frame = icv_alloc_picture_FFMPEG(AV_PIX_FMT_YUV420P, width, height, true);
	//AVFrame* av_frame_RGBA = icv_alloc_picture_FFMPEG(AV_PIX_FMT_RGBA, width, height, true);

	AVPacket av_packet;
	av_init_packet(&av_packet);
	av_packet.data = &(packetInput[HEADER_SIZE]);
	av_packet.size = apparentSize;

	int frame_finished = 0;
	int av_return = avcodec_decode_video2(av_codec_context, av_frame, &frame_finished, &av_packet);

	if (av_return <= 0 || !frame_finished)
		return;

	uint8_t* rgbaData[1] = { bts }; // RGBA have one plane
	int linesize[1] = { 4 * options->OutputWidth }; // RGBA stride

	//struct SwsContext* convert_context = sws_getContext(oWidth, oHeight, AV_PIX_FMT_YUV420P, oWidth, oHeight, AV_PIX_FMT_RGBA, SWS_FAST_BILINEAR, NULL, NULL, NULL);
	//sws_scale(convert_context, av_frame->data, av_frame->linesize, 0, height, av_frame_rgba->data, av_frame_rgba->linesize);
	sws_scale(convert_context, av_frame->data, av_frame->linesize, 0, oHeight, rgbaData, linesize);

	memcpy(bgraOutput, rgbaData[0], sz);
	
	//fprintf(stdout, "Pointer: %p", out);
	//int rgb_size = avpicture_get_size(AV_PIX_FMT_RGBA, width, height);
	//avpicture_layout((AVPicture *)av_frame_rgba, AV_PIX_FMT_RGBA, width, height, out, rgb_size);

	//av_packet.data = NULL;
	//av_packet.size = 0;
	//avcodec_decode_video2(av_codec_context, av_frame, &frame_finished, &av_packet);

	//x264_encoder_close(encoder);
	//free(NALBYTES);
	//sws_freeContext(convertCtx);
	//x264_picture_clean(&pic_in);

	//AVPacket emptyPacket;
	//av_init_packet(&emptyPacket);
	//emptyPacket.data = NULL;
	//emptyPacket.size = 0;
	//emptyPacket.stream_index = av_packet.stream_index;
	//avcodec_decode_video2(av_codec_context, av_frame, &frame_finished, &emptyPacket);
	//av_packet_unref(&emptyPacket);
	//fprintf(stdout, "%d", got_frame);

	//av_packet_unref(&av_packet);
	//av_free_packet(&av_packet);

	//av_freep(&av_frame_rgba->data[0]);
	//av_freep(&av_frame->data[0]);

	//av_free(av_codec_context);
	//av_freep(av_codec_context);
	//avcodec_free_context(av_codec_context);
	//av_free(av_codec_context);

	//free(outBytes);
	//x264_encoder_close(encoder);

	//free(NALBYTES);
	//Free();
	//avcodec_close(av_codec_context);

	clock_t end = clock();
	//double time_spent = ((double)(end - begin)) / CLOCKS_PER_SEC;
	//fprintf(stdout, "Time Spent: %f\n", time_spent);
}